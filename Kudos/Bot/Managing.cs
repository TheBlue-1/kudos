#region
using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.WebSocket;
#endregion

namespace Kudos.Bot {
	public sealed class Managing {
		public static Managing Instance { get; } = new Managing();

		static Managing() { }

		private Managing() { }

		public void Delete(ISocketMessageChannel channel, int count) {
			count = count < 1 ? 1 : count > 100 ? 100 : count;
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
