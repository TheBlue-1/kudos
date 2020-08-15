#region
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

		public async Task SendEmbed(IMessageChannel channel, EmbedBuilder embedBuilder) {
			await channel.SendMessageAsync(embed: embedBuilder.Build());
		}

		public async Task SendImage(IMessageChannel channel, string imageUrl) {
			EmbedBuilder builder = new EmbedBuilder().SetDefaults().WithImageUrl(imageUrl);
			await SendEmbed(channel, builder);
		}

		[Command("say", "says whatever you write behind say")]
		public async Task SendMessage([CommandParameter] IMessageChannel channel, [CommandParameter(0)] string text) {
			await SendEmbed(channel, new EmbedBuilder().SetDefaults().WithDescription(text));
		}

		[Command("ping", "shows the last pings")]
		public async Task SendPing([CommandParameter] ISocketMessageChannel channel) {
			await SendMessage(channel, PingMessage);
		}

		[Command("vote", "answers hello")]
		public async Task VoteLink([CommandParameter] ISocketMessageChannel channel, [CommandParameter] SocketUser user) {
			await SendMessage(channel, "Here is our vote link: https://top.gg/bot/719571683517792286/vote \nThank you for voting!");
		}
	}
}
