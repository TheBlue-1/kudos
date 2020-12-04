#region
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Kudos.Bot.Modules;
using Kudos.Models;
using Kudos.Utils;
using Settings = Kudos.Models.Settings;
#endregion

namespace Kudos.Bot {
	public class Client {
		/// <summary>
		///     TODO ideas/planned
		///     sleep rememberer (maybe general rememberer)
		///     achievements (to make people use features and do crazy stuff)
		/// </summary>
		private readonly DiscordSocketClient _client;
		private volatile bool _connected;

		private volatile bool _loggedIn;
		public FixedSizedQueue<int> LastPings = new FixedSizedQueue<int>(5);
		public IReadOnlyCollection<SocketGuild> Guilds => _client.Guilds;
		public string State => _loggedIn ? _connected ? _client.Status.ToString() : "connecting" : "logging in";
		private string Token { get; }

		public Client(string token) {
			Token = token;
			_client = new DiscordSocketClient();
		#pragma warning disable 162

			// ReSharper disable once ConditionIsAlwaysTrueOrFalse
			// ReSharper disable once UnreachableCode
			_client.SetGameAsync(Program.Debug ? "testing..." : $"with the '{new Settings()[SettingNames.Prefix].StringValue}help' command");
		#pragma warning restore 162
			_client.MessageReceived += ClientMessageReceived;
			_client.ReactionAdded += ClientReactionAdded;
			_client.LatencyUpdated += ClientLatencyUpdated;
			_client.MessageReceived += AutoResponseMessageReceived;
			_client.JoinedGuild += JoinedGuild;
			_client.UserVoiceStateUpdated += UserCallInteraction;
			_client.Disconnected += _ => {
				_connected = false;
				return Task.Run(() => { });
			};
			_client.LoggedOut += () => {
				_loggedIn = false;
				return Task.Run(() => { });
			};
			Start();
		}

		public event Action JoinedNewGuild;

		private static async Task AutoResponseMessageReceived(SocketMessage arg) {
			await Task.Run(async () => {
				try {
					await AutoResponse.Instance.Respond(arg);
				}
				catch (Exception e) {
					new ExceptionHandler(e, arg.Channel).Handle(false);
				}
			});
		}

		private Task ClientLatencyUpdated(int old, int val) {
			return Task.Run(() => { LastPings.Enqueue(val); });
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

		private async Task JoinedGuild(SocketGuild arg) {
			await Task.Run(() => { JoinedNewGuild?.Invoke(); });
			await Messaging.Instance.SendWelcomeMessage(arg);
		}

		[SuppressMessage("ReSharper", "InvertIf")]
		private void Start() {
			Task connector = new Task(async () => {
				while (true) {
					try {
						if (!_loggedIn) {
							await _client.LoginAsync(TokenType.Bot, Token);
							_loggedIn = true;
						}
						if (!_connected) {
							await _client.StartAsync();
							_connected = true;
						}
					}
					catch (Exception) {
						// ignored
					}
					await Task.Delay(5000);
				}

				// ReSharper disable once FunctionNeverReturns
			});
			connector.Start();
		}

		private static async Task UserCallInteraction(SocketUser user, SocketVoiceState oldState, SocketVoiceState newState) {
			await Task.Run(async () => {
				if (newState.VoiceChannel != null && oldState.VoiceChannel != newState.VoiceChannel) {
					await ServerGroupCalls.Instance.CheckEntering(user, newState.VoiceChannel);
				}
			});
		}
	}
}
