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
using Kudos.Extensions;

// ReSharper disable UnusedMember.Global
#endregion

namespace Kudos.Bot.Modules {
	[CommandModule("Reactions")]
	public sealed class Reactions {
		private static readonly Dictionary<char, IEmote> EmoteMap = new() {
			{ 'a', new Emoji("\U0001F1E6") },
			{ 'b', new Emoji("\U0001F1E7") },
			{ 'c', new Emoji("\U0001F1E8") },
			{ 'd', new Emoji("\U0001F1E9") },
			{ 'e', new Emoji("\U0001F1EA") },
			{ 'f', new Emoji("\U0001F1EB") },
			{ 'g', new Emoji("\U0001F1EC") },
			{ 'h', new Emoji("\U0001F1ED") },
			{ 'i', new Emoji("\U0001F1EE") },
			{ 'j', new Emoji("\U0001F1EF") },
			{ 'k', new Emoji("\U0001F1F0") },
			{ 'l', new Emoji("\U0001F1F1") },
			{ 'm', new Emoji("\U0001F1F2") },
			{ 'n', new Emoji("\U0001F1F3") },
			{ 'o', new Emoji("\U0001F1F4") },
			{ 'p', new Emoji("\U0001F1F5") },
			{ 'q', new Emoji("\U0001F1F6") },
			{ 'r', new Emoji("\U0001F1F7") },
			{ 's', new Emoji("\U0001F1F8") },
			{ 't', new Emoji("\U0001F1F9") },
			{ 'u', new Emoji("\U0001F1FA") },
			{ 'v', new Emoji("\U0001F1FB") },
			{ 'w', new Emoji("\U0001F1FC") },
			{ 'x', new Emoji("\U0001F1FD") },
			{ 'y', new Emoji("\U0001F1FE") },
			{ 'z', new Emoji("\U0001F1FF") }
		};
		public static Reactions Instance { get; } = new();

		static Reactions() { }

		private Reactions() { }

		[Command("react", "reacts with letters to latest message")]
		public async Task Delete([CommandParameter] SocketMessage message, [CommandParameter(0)] string text) {
			text = text.ToLower();
			if (!(text.JustNormalChars() && text.UniqueChars())) {
				throw new KudosArgumentException("Reaction text can just use a-z and each character just one time.");
			}
			await Managing.Instance.Delete(message.Channel);
			IMessage reactMessage = null;
			IAsyncEnumerable<IReadOnlyCollection<IMessage>> messageCollections = message.Channel.GetMessagesAsync(message, Direction.Before, 1);
			await messageCollections.ForEachAsync(messages => {
				if (messages.Count > 0) {
					reactMessage = messages.First();
				}
			});
			foreach (char c in text) {
				try {
					await reactMessage.AddReactionAsync(EmoteMap[c]);
				}
				catch (HttpException e) {
					if (e.HttpCode != HttpStatusCode.Forbidden) {
						throw;
					}
					throw new KudosArgumentException(
						"Kudos cant react to the message anymore (maximum different emojis reached / bot has insufficient rights)");
				}
			}
		}
	}
}
