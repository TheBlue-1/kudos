#region
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

		static Messaging() { }

		private Messaging() { }

		[Command("hello")]
		public async Task Hello([CommandParameter] ISocketMessageChannel channel, [CommandParameter] SocketUser user) {
			await SendMessage(channel, HelloText + user.Mention);
		}

		[Command("help")]
		public async Task Help([CommandParameter] ISocketMessageChannel channel) {
			await SendEmbed(channel, CommandModules.Instance.CommandListAsEmbed);
		}

		public async Task SendEmbed(IMessageChannel channel, EmbedBuilder embedBuilder) {
			await channel.SendMessageAsync(embed: embedBuilder.Build());
		}

		[Command("say")]
		public async Task SendMessage([CommandParameter] IMessageChannel channel, [CommandParameter(0)] string text) {
			await SendEmbed(channel, new EmbedBuilder().SetDefaults().WithDescription(text));
		}
	}
}
