#region
using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Kudos.Attributes;
using Kudos.Extensions;

// ReSharper disable UnusedMember.Global
#endregion

namespace Kudos.Bot.Modules {
	[CommandModule("Messaging")]
	public sealed class Messaging {
		private const string HelloText = "hello ";

		public static Messaging Instance { get; } = new Messaging();
		private static string PingMessage => Program.Client.LastPings.Aggregate("*Pong*\nMy last pings:\n", (current, ping) => current + (ping + "ms\n"));

		static Messaging() { }

		private Messaging() { }

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

		public async Task SendExpiringMessage(IMessageChannel channel, string text, TimeSpan timeSpan) {
			IUserMessage message = await SendMessage(channel, text);
			await Task.Delay(timeSpan);
			await message.DeleteAsync();
		}

		public async Task SendImage(IMessageChannel channel, string imageUrl) {
			EmbedBuilder builder = new EmbedBuilder().SetDefaults().WithImageUrl(imageUrl);
			await SendEmbed(channel, builder);
		}

		[Command("say", "says whatever you write behind say")]
		public async Task<IUserMessage> SendMessage([CommandParameter] IMessageChannel channel, [CommandParameter(0)] string text) =>
			await SendEmbed(channel, new EmbedBuilder().SetDefaults().WithDescription(text));

		[Command("ping", "shows the last pings")]
		public async Task SendPing([CommandParameter] ISocketMessageChannel channel) {
			await SendMessage(channel, PingMessage);
		}

		public async Task SendWelcomeMessage(SocketGuild guild) {
			string message =
				$"Hello,\n I'm Kudos and I was added to {guild.Name}.\n As an administrator you can use all of my features!\n You can also DM me. \n For a description and a list just type `k!help` or visit [Top.gg](https://top.gg/bot/719571683517792286)";
			foreach (SocketGuildUser guildUser in guild.Users) {
				if (!guildUser.IsGuildAdmin()) {
					continue;
				}
				try {
					await SendMessage(await guildUser.GetOrCreateDMChannelAsync(), message);
				}
				catch {
					// ignored
				}
			}
		}

		[Command("vote", "sends our vote links")]
		public async Task VoteLink([CommandParameter] ISocketMessageChannel channel, [CommandParameter] SocketUser user) {
			await SendMessage(channel,
				"Vote for our bot: [bot vote](https://top.gg/bot/719571683517792286/vote) \n"
				+ "Vote for our server: [server vote](https://top.gg/servers/631180888394301451/vote) \n"
				+ "Thank you for voting!");
		}
	}
}
