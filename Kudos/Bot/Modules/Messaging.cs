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
		public async Task Help([CommandParameter] ISocketMessageChannel channel) {
			await SendEmbed(channel, CommandModules.Instance.CommandListAsEmbed);
		}

		public async Task SendEmbed(IMessageChannel channel, EmbedBuilder embedBuilder) {
			await channel.SendMessageAsync(embed: embedBuilder.Build());
		}

		[Command("say", "says whatever you write behind say")]
		public async Task SendMessage([CommandParameter] IMessageChannel channel, [CommandParameter(0)] string text) {
			await SendEmbed(channel, new EmbedBuilder().SetDefaults().WithDescription(text));
		}

		[Command("ping", "shows the last pings")]
		public async Task SendPing([CommandParameter] ISocketMessageChannel channel) {
			await SendMessage(channel, PingMessage);
		}
	}
}
