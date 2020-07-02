#region
using System;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Kudos.Bot.Modules;
using Kudos.Models;
using Kudos.Utils;
#endregion

namespace Kudos.Bot {
	public class Client {
		/// <summary>
		///     TODO ideas/planned
		///     sleep rememberer (maybe general rememberer)
		///     achievements (to make people use features and do crazy stuff)
		///     stalker (listens in other channel and plays in yours)
		/// </summary>
		private readonly DiscordSocketClient _client;

		public FixedSizedQueue<int> LastPings = new FixedSizedQueue<int>(5);
		public string State => StartedSuccessful ? _client.Status.ToString() : "starting";

		private bool StartedSuccessful { get; set; }

		public Client(string token) {
			_client = new DiscordSocketClient();
			_client.SetGameAsync($"with the '{new Settings().Prefix.Value}help' command");
			_client.MessageReceived += ClientMessageReceived;
			_client.ReactionAdded += ClientReactionAdded;
			_client.LatencyUpdated += _client_LatencyUpdated;
			_client.MessageReceived += AutoReactionMessageReceived;
			Start(token);
		}

		private Task _client_LatencyUpdated(int old, int val) {
			return Task.Run(() => { LastPings.Enqueue(val); });
		}

		private static async Task AutoReactionMessageReceived(SocketMessage arg) {
			await Task.Run(async () => {
				try {
					await Reactions.Instance.AutoReact(arg);
				}
				catch (Exception e) {
					new ExceptionHandler(e, arg.Channel).Handle();
				}
			});
		}

		private static async Task ClientMessageReceived(SocketMessage arg) {
			await Task.Run(() => {
				MessageInterpreter interpreter = new MessageInterpreter(arg);
				if (interpreter.Executable) {
					interpreter.TryExecute();
				}
			});
		}

		private static async Task ClientReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3) {
			//unused

			await Task.Run(() => { });
		}

		public async Task<RestUser> GetRestUserById(ulong id) => await _client.Rest.GetUserAsync(id);
		public SocketUser GetSocketUserById(ulong id) => _client.GetUser(id);
		public SocketUser GetSocketUserByUsername(string username, string discriminator) => _client.GetUser(username, discriminator);

		private async void Start(string token) {
			await _client.LoginAsync(TokenType.Bot, token);
			await _client.StartAsync();
			StartedSuccessful = true;
		}
	}
}
