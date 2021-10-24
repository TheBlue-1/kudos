#region

using Discord;
using Discord.WebSocket;
using Kudos.Attributes;
using Kudos.Extensions;
using Kudos.Utils;
using System;
using System.Linq;
using System.Threading.Tasks;

// ReSharper disable UnusedMember.Global
#endregion

namespace Kudos.Bot.Modules {

    [CommandModule("Messaging")]
    public sealed class Messaging {
        private const string HelloText = "hello ";

        public static Messaging Instance { get; } = new();

        private static string PingMessage =>
            Program.Client.LastPings.Reverse().Aggregate("*Pong*\nMy last pings:\n", (current, ping) => current + (ping + "ms\n"));

        static Messaging() {
        }

        private Messaging() {
        }

        [Command("hello", "answers hello")]
        public async Task Hello([CommandParameter] ISocketMessageChannel channel, [CommandParameter] SocketUser user) {
            await SendMessage(channel, HelloText + user.Mention);
        }

        [Command("help", "shows all commands")]
        public async Task Help([CommandParameter] ISocketMessageChannel channel, [CommandParameter] SocketUser author) {
            await SendEmbed(channel, CommandModules.Instance.CommandListAsEmbed(author.IsBotAdmin()));
        }

        public async Task<IUserMessage> SendEmbed(IMessageChannel channel, EmbedBuilder embedBuilder) =>
            await channel.SendMessageAsync(embed: embedBuilder.Build());

        public async Task SendExpiringMessage(IMessageChannel channel, string text, TimeSpan timeSpan = default, IEmote[] reactions = null) {
            if (timeSpan == default) {
                timeSpan = new TimeSpan(0, 0, 3);
            }
            IUserMessage message = await SendMessage(channel, text);
            if (reactions != null) {
                await message.AddReactionsAsync(reactions);
            }
            await Task.Delay(timeSpan);
            try {
                await message.DeleteAsync();
            } catch (Exception e) {
                LogService.Instance.Log($"A message could not be deleted\n{e}", LogService.LogType.Main, Google.Cloud.Logging.Type.LogSeverity.Notice);
            }
        }

        public async Task SendHonorMessage(IMessageChannel channel, SocketUser mentionedUser) {
            await SendExpiringMessage(channel,
                $"Hey, do you want to honor {mentionedUser}? Select number of honor points! (this message will delete itself in 10 sec)",
                new TimeSpan(0, 0, 10), Honor.HonorEmojis);
        }

        public async Task SendImage(IMessageChannel channel, string imageUrl) {
            EmbedBuilder builder = new EmbedBuilder().SetDefaults().WithImageUrl(imageUrl);
            await SendEmbed(channel, builder);
        }

        [Command("say", "says whatever you write behind say")]
        public async Task<IUserMessage> SendMessage([CommandParameter] IMessageChannel channel, [CommandParameter(0)] string text) {
            if (text.StartsWith('=')) {
                if (text.Length > 6000) {
                    text = text.Remove(6000 - 1 - 3) + "...";
                }
                return await channel.SendMessageAsync(text.Substring(1));
            }

            switch (text.Length) {
                //25 for field names,55 for footer+title
                case > 6000 - 25 - 55:
                text = text.Remove(6000 - 25 - 55 - 1 - 3) + "...";
                break;

                case <= 2048: return await SendEmbed(channel, new EmbedBuilder().SetDefaults().WithDescription(text));
            }
            EmbedBuilder builder = new EmbedBuilder().SetDefaults();
            string[] textParts = text.SplitAtSpace(25, 1024);
            foreach (string part in textParts) {
                builder.AddField(new EmbedFieldBuilder { Value = part, Name = "\u200b" });
            }
            return await SendEmbed(channel, builder);
        }

        [Command("ping", "shows the last pings")]
        public async Task SendPing([CommandParameter] ISocketMessageChannel channel) {
            await SendMessage(channel, PingMessage);
        }

        public async Task<int> SendToAdmins(string message) {
            int notReceived = 0;
            foreach (SocketGuild guild in Program.Client.Guilds) {
                foreach (SocketGuildUser guildUser in guild.Users) {
                    if (guildUser.IsBot || !guildUser.IsGuildAdmin()) {
                        continue;
                    }
                    try {
                        await SendMessage(await guildUser.GetOrCreateDMChannelAsync(), message);
                    } catch (Exception e) {
                        notReceived++;
                        if (e.GetType() == typeof(Exception)) { }
                    }
                }
            }
            return notReceived;
        }

        public async Task SendWelcomeMessage(SocketGuild guild) {
            string message =
                $"Hello,\n I'm Kudos and I was added to {guild.Name}.\n As an administrator you can use all of my features!\n You can also DM me. \n For a description and a list just type `k!help` or visit [Top.gg](https://top.gg/bot/719571683517792286) \n If you like the Bot please give us 5 Stars at [Top.gg Reviews](https://top.gg/bot/719571683517792286#reviews)";
            foreach (SocketGuildUser guildUser in guild.Users) {
                if (guildUser.IsBot || !guildUser.IsGuildAdmin()) {
                    continue;
                }
                try {
                    await SendMessage(await guildUser.GetOrCreateDMChannelAsync(), message);
                } catch (Exception e) {
                    LogService.Instance.Log($"A welcome message could not be delivered\n{e}", LogService.LogType.Main, Google.Cloud.Logging.Type.LogSeverity.Notice);
                }
            }
        }

        [Command("vote", "sends our vote links")]
        public async Task VoteLink([CommandParameter] ISocketMessageChannel channel) {
            await SendMessage(channel,
                "Vote for our bot: [bot vote](https://top.gg/bot/719571683517792286/vote) \n"
                + "Vote for our server: [server vote](https://top.gg/servers/631180888394301451/vote) \n"
                + "Thank you for voting!");
        }
    }
}