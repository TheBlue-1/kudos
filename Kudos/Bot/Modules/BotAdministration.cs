#region

using Discord.WebSocket;
using DiscordBotsList.Api.Objects;
using Kudos.Attributes;
using Kudos.DatabaseModels;
using Kudos.Exceptions;
using Kudos.Extensions;
using Kudos.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

// ReSharper disable UnusedMember.Global
#endregion

namespace Kudos.Bot.Modules {

    [CommandModule("BotAdministration", Accessibility.Hidden)]
    public sealed class BotAdministration {
        private DatabaseSyncedList<BanData> Bans { get; } = DatabaseSyncedList.Instance<BanData>();
        public static BotAdministration Instance { get; } = new();

        static BotAdministration() {
        }

        private BotAdministration() {
        }

        [Command("ban", "bans a person")]
        public async Task Ban([CommandParameter] ISocketMessageChannel channel, [CommandParameter(0)] SocketUser user, [CommandParameter(1)] bool hardBan) {
            Bans.Add(new BanData { UserId = user.Id, HardBan = hardBan });
            if (hardBan) {
                IEnumerable<HonorData> honors = DatabaseSyncedList.Instance<HonorData>().Where(h => h.Honorer == user.Id || h.Honored == user.Id);
                IEnumerable<QuestionData> questions = DatabaseSyncedList.Instance<QuestionData>().Where(q => q.Questionnaire == user.Id);
                IEnumerable<TimerData> timers = DatabaseSyncedList.Instance<TimerData>().Where(t => t.OwnerId == user.Id);
                foreach (HonorData honorData in honors) {
                    DatabaseSyncedList.Instance<HonorData>().Remove(honorData);
                }
                foreach (QuestionData questionData in questions) {
                    DatabaseSyncedList.Instance<QuestionData>().Remove(questionData);
                }
                foreach (TimerData timerData in timers) {
                    DatabaseSyncedList.Instance<TimerData>().Remove(timerData);
                }
            }
            await Messaging.Instance.SendExpiringMessage(channel, "banned", new TimeSpan(0, 0, 15));
        }

        [Command("checkuser", "checks if a user is known by the bot")]
        public async Task CheckUser([CommandParameter] ISocketMessageChannel channel, [CommandParameter(0)] SocketUser user) {
            SocketGuild[] guilds = Program.Client.Guilds.Where(g => g != null).OrderBy(guild => guild.Name).ToArray();
            IEnumerable<SocketGuildUser> users = guilds.SelectMany(g => g.Users);
            IEnumerable<SocketGuild> servers = users.Where(u => u.Id == user.Id).Select(u => u.Guild).ToArray();
            if (!servers.Any()) {
                await Messaging.Instance.SendMessage(channel, "User is not known");
                return;
            }
            string message = servers.Aggregate("User is known from the following servers",
                (current, socketGuild) => current + $"\n{socketGuild.Name}|{socketGuild.Id}");
            await Messaging.Instance.SendMessage(channel, message);
        }

        [Command("adminmsg", "sends a message to all guild admins")]
        public async Task MessageAdmins([CommandParameter] ISocketMessageChannel channel, [CommandParameter(0)] string message) {
            int notReceived = await Messaging.Instance.SendToAdmins(message);
            await Messaging.Instance.SendMessage(channel, $"sent successfully. {notReceived} didn't receive the message");
        }

        [Command("restart", "restarts the bot")]
        public async Task Restart([CommandParameter] ISocketMessageChannel channel) {
            Process.Start(Assembly.GetExecutingAssembly().Location);
            await Messaging.Instance.SendMessage(channel, "Started new Instance. Exiting now.");
            Environment.Exit(0);
        }

        [Command("guilds", "shows all guilds of the bot")]
        public async Task SendGuilds([CommandParameter] ISocketMessageChannel channel) {
            SocketGuild[] guilds = Program.Client.Guilds.Where(g => g != null).OrderBy(guild => guild.Name).ToArray();
            IEnumerable<SocketGuildUser> users = guilds.SelectMany(g => g.Users).GroupBy(user => user.Id).Select(group => group.First()).ToArray();
            string message = $"I am present on {guilds.Length} guilds with a total of {users.Count()} users ({users.Count(user => user.IsBot)} are bots)";
            if (!Program.IsBotListBot) {
                message += "\nI am not the real Kudos (just a test/beta version)";
            }
            message = guilds.Aggregate(message,
                (current, guild) =>
                    current
                    + $"\n({guild.Users?.Count}|{guild.Users?.Where(u => u.IsBot).Count()}|{guild.Users?.Where(u => u.IsGuildAdmin()).Count()}) {guild.Name} [{guild.Id}] ({guild.Owner?.Id})");

            await Messaging.Instance.SendMessage(channel, message);
        }

        [Command("gleaders", "shows the global leader board")]
        public async Task SendLeaders([CommandParameter] ISocketMessageChannel channel, [CommandParameter(0, 0, 0)] TimeSpan time) {
            await Messaging.Instance.SendEmbed(channel, Honor.Instance.GuildStatsEmbed(null, time));
        }

        [Command("votes", "shows who voted for the bot")]
        public async Task SendVotes([CommandParameter] ISocketMessageChannel channel) {
            if (Program.BotList == null) {
                throw new KudosUnauthorizedException("bot is not allowed to see this information");
            }
            List<IDblEntity> voters = await Program.BotList.ThisBot.GetVotersAsync();
            var countedVotes = voters.GroupBy(voter => voter.Id).Select(voter => new { voter.First().Id, Count = voter.Count(), voter.First().Username });

            var countedVotesList = countedVotes.ToList();
            string message =
                $"Total Votes: {Program.BotList.ThisBot.Points}\nThis Month Votes: coming soon\nVoters Count: {countedVotesList.Count}\nTheir total votes: {voters.Count}";
            message = countedVotesList.Aggregate(message, (current, voter) => current + $"\n{voter.Username} ({voter.Count})");
            await Messaging.Instance.SendMessage(channel, message);
        }

        [Command("unban", "unbans a person")]
        public async Task UnBan([CommandParameter] ISocketMessageChannel channel, [CommandParameter(0)] SocketUser user) {
            BanData ban = Bans.FirstOrDefault(b => b.UserId == user.Id);
            if (ban == null) {
                throw new KudosArgumentException("not banned");
            }
            if (ban.HardBan) {
                throw new KudosArgumentException("hard banned");
            }

            Bans.Remove(ban);
            await Messaging.Instance.SendExpiringMessage(channel, "unbanned", new TimeSpan(0, 0, 15));
        }

        [Command("update", "updates the bot (currently only master-version)")]
        public async Task UpdateBot([CommandParameter] ISocketMessageChannel channel) {
            const string updateFileKey = "updateFile";
            const string pullFileKey = "pullFile";
            AsyncThreadsafeFileSyncedDictionary<string, string> settings = FileService.Instance.Settings;
            if (!settings.ContainsKey(pullFileKey) || string.IsNullOrEmpty(settings[pullFileKey])) {
                throw new KudosInvalidOperationException($"no pull file path specified in settings file (key:'{pullFileKey}')");
            }
            if (!settings.ContainsKey(updateFileKey) || string.IsNullOrEmpty(settings[updateFileKey])) {
                throw new KudosInvalidOperationException($"no update file path specified in settings file (key:'{updateFileKey}')");
            }
            await Messaging.Instance.SendMessage(channel, "Paths found");

            Process permProcess = new() {
                StartInfo = new ProcessStartInfo("/bin/bash") {
                    Arguments = $"-c \"sudo chmod 777 {settings[pullFileKey]}",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            permProcess.Start();
            await permProcess.WaitForExitAsync();
            string permError = await permProcess.StandardError.ReadToEndAsync();
            string permOutput = await permProcess.StandardOutput.ReadToEndAsync();
            await Messaging.Instance.SendMessage(channel, $"Gave permission to pull file\nLog:\n{permOutput}\nError:\n{permError}");
            Process pullProcess = new() {
                StartInfo = new ProcessStartInfo(settings[pullFileKey]) { RedirectStandardError = true, RedirectStandardOutput = true, CreateNoWindow = true }
            };
            pullProcess.Start();
            await pullProcess.WaitForExitAsync();
            string pullError = await pullProcess.StandardError.ReadToEndAsync();
            string pullOutput = await pullProcess.StandardOutput.ReadToEndAsync();
            await Messaging.Instance.SendMessage(channel, $"Bot pulled\nLog:\n{pullOutput}\nError:\n{pullError}");
            Process updateProcess = new() {
                StartInfo = new ProcessStartInfo(settings[updateFileKey]) { RedirectStandardError = true, RedirectStandardOutput = true, CreateNoWindow = true }
            };
            updateProcess.Start();
            await Messaging.Instance.SendMessage(channel, "started update (if the update is a success there will be no further messages)");

            //Kudos should normally be killed here
            await updateProcess.WaitForExitAsync();
            string updateError = await updateProcess.StandardError.ReadToEndAsync();
            string updateOutput = await updateProcess.StandardOutput.ReadToEndAsync();
            await Messaging.Instance.SendMessage(channel, $"Bot updated\nLog:\n{updateOutput}\nError:\n{updateError}");
        }

        [Command("wait", "waits the given time and sends a response after that")]
        public async Task WaitAndRespond([CommandParameter] ISocketMessageChannel channel, [CommandParameter(0)] TimeSpan span) {
            await Task.Delay(span);
            await Messaging.Instance.SendMessage(channel, $"waited for {span}");
        }
    }
}