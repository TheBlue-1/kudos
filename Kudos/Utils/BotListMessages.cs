#region
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Kudos.Exceptions;
using UserExtensions = Kudos.Extensions.UserExtensions;
#endregion

namespace Kudos.Utils {
	public class BotListMessages {
		private const ulong BotListGuildId = 264445053596991498;
		private const ulong BotListLogChannelId = 264889891039608874;

		private readonly Regex _added = new Regex("$.+ added bot .+^");
		private readonly Regex _approved = new Regex("$.+ by .+ was approved by .+\\..+^");
		private readonly Regex _declined = new Regex("$Bot .+ by .+ was declined\\.^");

		private readonly List<IMessage> _messageList = new List<IMessage>();

		private SocketUser _lastChecked;
		private IMessage _lastMessage;
		private bool _loaded;
		private static ISocketMessageChannel LogChannel => Program.Client.GetSocketGuildById(BotListGuildId).GetTextChannel(BotListLogChannelId);

		public List<SocketUser> QueuedBots {
			get {
				if (!_loaded) {
					throw new KudosInternalException("used Queued bots before loading finished");
				}
				return ((IEnumerable<SocketUser>)ReversedBotQueue).Reverse().ToList();
			}
		}
		private List<SocketUser> ReversedBotQueue { get; } = new List<SocketUser>();

		private static SocketUser BotAt(string message, int index) {
			string[] contentParts = message.Split(' ');
			SocketUser bot = UserExtensions.FromMention(contentParts[index]);
			return bot;
		}

		[SuppressMessage("ReSharper", "InvertIf")]
		private SocketUser LastCheckedBot() {
			foreach (IMessage message in _messageList) {
				if (_approved.IsMatch(message.Content)) {
					SocketUser bot = BotAt(message.Content, 0);
					if (bot != null) {
						return bot;
					}
				}
				if (_declined.IsMatch(message.Content)) {
					SocketUser bot = BotAt(message.Content, 1);
					if (bot != null) {
						return bot;
					}
				}
			}
			return null;
		}

		public async Task LoadMessages() {
			_loaded = false;
			_lastMessage = null;
			_messageList.Clear();
			_lastChecked = null;
			for (int i = 0; i < 3; i++) {
				await LoadNextMessages();
				_lastChecked = LastCheckedBot();
				if (_lastChecked != null) {
					break;
				}
			}
			if (_lastChecked != null) {
				throw new KudosInternalException("last checked bot not found");
			}

			if (LookForAddings(_messageList)) {
				_loaded = true;
				return;
			}

			for (int i = 0; i < 25; i++) {
				List<IMessage> newMessages = await LoadNextMessages();

				if (!LookForAddings(newMessages)) {
					continue;
				}
				_loaded = true;
				return;
			}
		}

		private async Task<List<IMessage>> LoadNextMessages() {
			IAsyncEnumerable<IReadOnlyCollection<IMessage>> messageCollections =
				_lastMessage == null ? LogChannel.GetMessagesAsync() : LogChannel.GetMessagesAsync(_lastMessage, Direction.Before);
			List<IMessage> newMessages = new List<IMessage>();
			await messageCollections.ForEachAsync(messageCollection => {
				_messageList.AddRange(messageCollection.Cast<SocketMessage>());
				newMessages.AddRange(messageCollection);
			});
			return newMessages;
		}

		private bool LookForAddings(IEnumerable<IMessage> messages) {
			foreach (IMessage message in messages) {
				if (!_added.IsMatch(message.Content)) {
					continue;
				}
				SocketUser bot = BotAt(message.Content, 4);
				if (bot == null) {
					continue;
				}

				if (bot.Id == _lastChecked.Id) {
					return true;
				}
				ReversedBotQueue.Add(bot);
			}
			return false;
		}
	}
}
