#region
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
		private string _lastState;

		private volatile bool _loggedIn;
		public FixedSizedQueue<int> LastPings = new FixedSizedQueue<int>(5);

		public ulong BotUserId => _client.CurrentUser.Id;
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
				StateChange();
				return Task.Run(() => { });
			};
			_client.LoggedOut += () => {
				_loggedIn = false;
				StateChange();
				return Task.Run(() => { });
			};
		}

		public event Action JoinedNewGuild;

		public event EventHandler<StateChangedData> StateChanged;

		private static Task AutoResponseMessageReceived(SocketMessage arg) {
			_ = Task.Run(async () => {
				try {
					await AutoResponse.Instance.Respond(arg);
				}
				catch (Exception e) {
					new ExceptionHandler(e, arg.Channel).Handle(false);
				}
			});
			return Task.Run(() => true);
		}

		private Task ClientLatencyUpdated(int old, int val) {
			_ = Task.Run(() => { LastPings.Enqueue(val); });

			return Task.Run(() => true);
		}

		private static Task ClientMessageReceived(SocketMessage arg) {
			_ = Task.Run(() => {
				try {
					MessageInterpreter interpreter = new MessageInterpreter(arg);
					if (interpreter.Executable) {
						interpreter.TryExecute();
					}
				}
				catch (Exception e) {
					new ExceptionHandler(e, arg.Channel).Handle(false);
				}
			});
			return Task.Run(() => true);
		}

		private static Task ClientReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3) {
			_ = Task.Run(async () => {
				try {
					if (!arg1.HasValue) {
						return;
					}
					IUserMessage message = arg1.Value;
					if (message?.Author?.Id == Program.Client.BotUserId && arg3.UserId != Program.Client.BotUserId) {
						await Honor.Instance.HonorUserWithReaction(message, arg3);
					}
				}
				catch (Exception e) {
					new ExceptionHandler(e, arg3.Channel).Handle(false);
				}
			});
			return Task.Run(() => true);
		}

		public async Task<RestUser> GetRestUserById(ulong id) => await _client.Rest.GetUserAsync(id);
		public SocketRole GetRoleById(ulong id) => _client.Guilds.SelectMany(guild => guild.Roles).FirstOrDefault(role => role.Id == id);

		public SocketUser GetSocketUserById(ulong id) => _client.GetUser(id);

		public SocketUser GetSocketUserByUsername(string username, string discriminator) => _client.GetUser(username, discriminator);

		private Task JoinedGuild(SocketGuild arg) {
			_ = Task.Run(async () => {
				try {
					JoinedNewGuild?.Invoke();
					await Messaging.Instance.SendWelcomeMessage(arg);
				}
				catch (Exception e) {
					new ExceptionHandler(e, arg.DefaultChannel).Handle(false);
				}
			});

			return Task.Run(() => true);
		}

		[SuppressMessage("ReSharper", "InvertIf")]
		public void Start() {
			StateChange();
			Task connector = new Task(async () => {
				while (true) {
					try {
						if (!_loggedIn) {
							await _client.LoginAsync(TokenType.Bot, Token);
							_loggedIn = true;
							StateChange();
						}
						if (!_connected) {
							await _client.StartAsync();
							_connected = true;
							StateChange();
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

		private void StateChange() {
			if (State == _lastState) {
				return;
			}

			_lastState = State;
			StateChanged?.Invoke(this, State);
		}

		private static Task UserCallInteraction(SocketUser user, SocketVoiceState oldState, SocketVoiceState newState) {
			_ = Task.Run(async () => {
				try {
					//entering
					if (newState.VoiceChannel != null && oldState.VoiceChannel != newState.VoiceChannel) {
						await ServerGroupCalls.Instance.CheckEntering(user, newState.VoiceChannel);
					}

					//leaving
					if (oldState.VoiceChannel != null && oldState.VoiceChannel != newState.VoiceChannel) {
						await ServerGroupCalls.Instance.CheckLeaving(user, oldState.VoiceChannel);
					}
				}
				catch (Exception e) {
					new ExceptionHandler(e, null).Handle(false);
				}
			});
			return Task.Run(() => true);
		}

		public class StateChangedData {
			private string _value;
			public static implicit operator string(StateChangedData s) => s._value;
			public static implicit operator StateChangedData(string s) => new StateChangedData { _value = s };
		}
	}
}
