#region
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using DiscordBotsList.Api.Objects;
using Kudos.Attributes;
using Kudos.DatabaseModels;
using Kudos.Exceptions;
using Kudos.Extensions;
using Kudos.Utils;

// ReSharper disable UnusedMember.Global
#endregion

namespace Kudos.Bot.Modules {
	[CommandModule("BotAdministration", Accessibility.Hidden)]
	public sealed class BotAdministration {
		private DatabaseSyncedList<BanData> Bans { get; } = DatabaseSyncedList.Instance<BanData>();
		public static BotAdministration Instance { get; } = new();

		static BotAdministration() { }

		private BotAdministration() { }

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

		[Command("guilds", "shows all guilds of the bot")]
		public async Task SendGuilds([CommandParameter] ISocketMessageChannel channel) {
			SocketGuild[] guilds = Program.Client.Guilds.Where(g => g != null).OrderBy(guild => guild.Name).ToArray();
			IEnumerable<SocketGuildUser> users = guilds.SelectMany(g => g.Users).GroupBy(user => user.Id).Select(group => group.First()).ToArray();
			string message = $"I am present on {guilds.Length} guilds with a total of {users.Count()} users ({users.Where(user => user.IsBot)} are bots)";
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

		[Command("wait", "waits the given time and sends a response after that")]
		public async Task WaitAndRespond([CommandParameter] ISocketMessageChannel channel, [CommandParameter(0)] TimeSpan span) {
			await Task.Delay(span);
			await Messaging.Instance.SendMessage(channel, $"waited for {span}");
		}
	}
}
