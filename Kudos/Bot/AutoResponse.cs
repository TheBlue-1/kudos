#region
using System.Collections.Immutable;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Kudos.Bot.Modules;
using Kudos.Extensions;
#endregion

namespace Kudos.Bot {
	public sealed class AutoResponse {
		public static AutoResponse Instance { get; } = new AutoResponse();

		static AutoResponse() { }

		private AutoResponse() { }

		private static async Task AutoImage(SocketMessage message) {
			ImmutableDictionary<string, string> images = message.Settings().AutoImage.Value;
			foreach ((string needle, string imageUrl) in images) {
				if (NeedleContained(message.Content, needle)) {
					await Messaging.Instance.SendImage(message.Channel, imageUrl);
				}
			}
		}

		private static async Task AutoMessage(SocketMessage message) {
			ImmutableDictionary<string, string> messages = message.Settings().AutoMessage.Value;
			foreach ((string needle, string responseMessage) in messages) {
				if (NeedleContained(message.Content, needle)) {
					await Messaging.Instance.SendMessage(message.Channel, responseMessage);
				}
			}
		}

		private static async Task AutoReact(SocketMessage message) {
			ImmutableDictionary<string, string> reactions = message.Settings().AutoReact.Value;
			foreach ((string needle, string emojiString) in reactions) {
				if (string.IsNullOrWhiteSpace(emojiString)) {
					continue;
				}
				IEmote emoji;
				if (Emote.TryParse(emojiString, out Emote emote)) {
					emoji = emote;
				} else {
					emoji = new Emoji(emojiString);
				}
				if (NeedleContained(message.Content, needle)) {
					await message.AddReactionAsync(emoji);
				}
			}
		}

		private static bool NeedleContained(string haystack, string needle) {
			if (needle.StartsWith("*")) {
				if (needle.EndsWith("*")) {
					if (haystack.Contains(needle.Substring(1, needle.Length - 2))) {
						return true;
					}
				}
				if (haystack.EndsWith(needle.Substring(1))) {
					return true;
				}
			}
			// ReSharper disable once InvertIf
			if (needle.EndsWith("*")) {
				if (haystack.StartsWith(needle.Substring(0, needle.Length - 1))) {
					return true;
				}
			}
			return haystack == needle;
		}

		public async Task Respond(SocketMessage message) {
			if (message.Settings().AutoResponses.Value) {
				await AutoMessage(message);
				await AutoImage(message);
				await AutoReact(message);
			}
		}
	}
}
