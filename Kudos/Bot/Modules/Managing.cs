#region
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Kudos.Attributes;
using Kudos.Exceptions;

// ReSharper disable UnusedMember.Global
#endregion

namespace Kudos.Bot.Modules {
	[CommandModule("Managing")]
	public sealed class Managing {
		public static Managing Instance { get; } = new();

		static Managing() { }

		private Managing() { }

		public async Task Delete(ISocketMessageChannel channel, int count = 1) {
			IAsyncEnumerable<IReadOnlyCollection<IMessage>> messageCollections = channel.GetMessagesAsync(count);

			await messageCollections.ForEachAwaitAsync(async messages => {
				foreach (IMessage message in messages) {
					try {
						await channel.DeleteMessageAsync(message);
					}
					catch (HttpException e) {
						if (e.HttpCode == HttpStatusCode.NotFound) {
							throw new KudosInvalidOperationException("a message you wanted me to delete was already deleted (I stopped deleting)",
								"tried to delete msg already deleted");
						}
					}
				}
			});
		}

		[Command("delete", "deletes messages in the channel", Accessibility.Admin)]
		public async Task DeletePerCommand([CommandParameter] ISocketMessageChannel channel, [CommandParameter(0, 1, 1, 100)] int count) {
			count++;
			await Delete(channel, count);
		}
	}
}
