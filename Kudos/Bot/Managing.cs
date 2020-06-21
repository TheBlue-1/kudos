#region
using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.WebSocket;
using Kudos.Attributes;

// ReSharper disable UnusedMember.Global
#endregion

namespace Kudos.Bot {
	[CommandModule("Managing")]
	public sealed class Managing {
		public static Managing Instance { get; } = new Managing();

		static Managing() { }

		private Managing() { }

		[Command("delete")]
		public void Delete([CommandParameter] ISocketMessageChannel channel, [CommandParameter(0, 1, 1, 100)] int count) {
			count++;
			IAsyncEnumerable<IReadOnlyCollection<IMessage>> messageCollections = channel.GetMessagesAsync(count);
			messageCollections.ForEachAsync(messages => {
				foreach (IMessage message in messages) {
					channel.DeleteMessageAsync(message);
				}
			});
		}
	}
}
