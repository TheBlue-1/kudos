#region
using System.Threading.Tasks;
using Discord.WebSocket;
using Kudos.Attributes;
using Kudos.Utils;

// ReSharper disable UnusedMember.Global
#endregion

namespace Kudos.Bot.Modules {
	[CommandModule("Information")]
	public sealed class Information {
		public static Information Instance { get; } = new Information();

		static Information() { }

		private Information() { }

		// ReSharper disable once StringLiteralTypo
		[Command("botlistqueue", "shows the place of your bot in the queue for top.gg", true)]
		public async Task DeletePerCommand([CommandParameter] ISocketMessageChannel channel, [CommandParameter(0)] SocketUser bot) {
			BotListMessages messages = new BotListMessages();

			await messages.LoadMessages();
			int index = messages.QueuedBots.IndexOf(bot);
			await Messaging.Instance.SendMessage(channel, index > 0 ? $"your current place in queue is {index + 1}" : "your bot is not in the queue");
		}

		// ReSharper disable once StringLiteralTypo
		[Command("servercount", "shows on how many serves this bot is")]
		public async Task ServerCount([CommandParameter] ISocketMessageChannel channel) {
			await Messaging.Instance.SendMessage(channel, $"I am active on {Program.Client.ServerCount} servers");
		}
	}
}
