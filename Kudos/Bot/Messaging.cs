#region
using Discord.WebSocket;
using Kudos.Attributes;

// ReSharper disable UnusedMember.Global
#endregion

namespace Kudos.Bot {
	[CommandModule("Messaging")]
	public sealed class Messaging {
		private const string HelloText = "hello ";

		public static Messaging Instance { get; } = new Messaging();

		static Messaging() { }

		private Messaging() { }

		[Command("hello")]
		public void Hello([CommandParameter] ISocketMessageChannel channel, [CommandParameter] SocketUser user) {
			channel.SendMessageAsync(HelloText + user.Mention);
		}

		[Command("help")]
		public void Help([CommandParameter] ISocketMessageChannel channel) {
			channel.SendMessageAsync(CommandModules.Instance.ToString());
		}

		[Command("say")]
		public void Message([CommandParameter] ISocketMessageChannel channel, [CommandParameter(0)] string text) {
			channel.SendMessageAsync(text);
		}
	}
}
