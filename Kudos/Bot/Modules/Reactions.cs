#region
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Kudos.Attributes;
using Kudos.Exceptions;
using Kudos.Extensions;

// ReSharper disable UnusedMember.Global
#endregion

namespace Kudos.Bot.Modules {
	[CommandModule("Reactions")]
	public sealed class Reactions {
		private static readonly Dictionary<char, IEmote> EmoteMap = new Dictionary<char, IEmote> {
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
		public static Reactions Instance { get; } = new Reactions();

		static Reactions() { }

		private Reactions() { }

		public async Task AutoReact(SocketMessage message) {
			string content = message.Content;
			ImmutableDictionary<string, string> reactions = message.Settings().AutoReact.Value;
			foreach ((string needle, string emojiString) in reactions) {
				if (string.IsNullOrWhiteSpace(emojiString)) {
					continue;
				}
				Emoji emoji = new Emoji(emojiString);
				if (needle.StartsWith("*")) {
					if (needle.EndsWith("*")) {
						if (content.Contains(needle.Substring(1, needle.Length - 2))) {
							await message.AddReactionAsync(emoji);
						}
						continue;
					}
					if (content.EndsWith(needle.Substring(1))) {
						await message.AddReactionAsync(emoji);
					}
					continue;
				}
				if (needle.EndsWith("*")) {
					if (content.StartsWith(needle.Substring(0, needle.Length - 1))) {
						await message.AddReactionAsync(emoji);
					}
					continue;
				}
				if (content == needle) {
					await message.AddReactionAsync(emoji);
				}
			}
		}

		[Command("react", "reacts with letters to latest message")]
		public async Task Delete([CommandParameter] SocketMessage message, [CommandParameter(0)] string text) {
			text = text.ToLower();
			if (!(text.JustNormalChars() && text.UniqueChars())) {
				throw new KudosArgumentException("Reaction text can just use a-z and each character just one time.");
			}
			Managing.Instance.Delete(message.Channel);
			IMessage reactMessage = null;
			IAsyncEnumerable<IReadOnlyCollection<IMessage>> messageCollections = message.Channel.GetMessagesAsync(message, Direction.Before, 1);
			await messageCollections.ForEachAsync(messages => {
				if (messages.Count > 0) {
					reactMessage = messages.First();
				}
			});
			foreach (char c in text) {
				await reactMessage.AddReactionAsync(EmoteMap[c]);
			}
		}
	}
}
