﻿#region
using System.Collections.Immutable;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Kudos.Bot.Modules;
using Kudos.Extensions;
using Kudos.Models;
#endregion

namespace Kudos.Bot {
	public sealed class AutoResponse {
		public static AutoResponse Instance { get; } = new AutoResponse();

		static AutoResponse() { }

		private AutoResponse() { }

		private static async Task AutoImage(SocketMessage message) {
			message.Settings()[SettingNames.AutoImage].Value(out ImmutableDictionary<string, string> images);
			foreach ((string needle, string imageUrl) in images) {
				if (NeedleContained(message.Content, needle)) {
					await Messaging.Instance.SendImage(message.Channel, imageUrl);
				}
			}
		}

		private static async Task AutoMessage(SocketMessage message) {
			message.Settings()[SettingNames.AutoMessage].Value(out ImmutableDictionary<string, string> messages);
			foreach ((string needle, string responseMessage) in messages) {
				if (NeedleContained(message.Content, needle)) {
					await Messaging.Instance.SendMessage(message.Channel, responseMessage);
				}
			}
		}

		private static async Task AutoReact(SocketMessage message) {
			message.Settings()[SettingNames.AutoReact].Value(out ImmutableDictionary<string, IEmote> reactions);
			foreach ((string needle, IEmote emoji) in reactions) {
				string emojiString = emoji.Name;
				if (string.IsNullOrWhiteSpace(emojiString)) {
					continue;
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
			message.Settings()[SettingNames.AutoResponses].Value(out bool doRespond);
			if (doRespond) {
				await AutoMessage(message);
				await AutoImage(message);
				await AutoReact(message);
			}
		}
	}
}