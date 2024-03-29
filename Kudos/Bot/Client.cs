﻿#region

using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Kudos.Bot.Modules;
using Kudos.Extensions;
using Kudos.Models;
using Kudos.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using LogSeverity = Google.Cloud.Logging.Type.LogSeverity;
using Settings = Kudos.Models.Settings;

#endregion

namespace Kudos.Bot {

    public class Client {
        private readonly DiscordSocketClient _client;
        private volatile bool _connected;
        private string _lastState;

        private volatile bool _loggedIn;
        public FixedSizedQueue<int> LastPings { get; } = new(5);

        public ulong BotUserId => _client.CurrentUser.Id;
        private static Task FakeTask => Task.Run(() => true);
        public IReadOnlyCollection<SocketGuild> Guilds => _client.Guilds;

        public IEnumerable<GuildPermission> Permissions { get; } = new[] {
            GuildPermission.SendMessages, GuildPermission.ViewChannel, GuildPermission.UseExternalEmojis, GuildPermission.ManageMessages,
            GuildPermission.AddReactions, GuildPermission.ReadMessageHistory
        };

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
            _client.LeftGuild += LeftGuild;

            _client.UserVoiceStateUpdated += UserCallInteraction;
            _client.Ready += Init;
            _client.Disconnected += _ => {
                _connected = false;
                StateChange();
                return FakeTask;
            };
            _client.LoggedOut += () => {
                _loggedIn = false;
                StateChange();
                return FakeTask;
            };
        }

        public event Action GuildCountChanged;

        public event EventHandler<StateChangedData> StateChanged;

        private static Task AutoResponseMessageReceived(SocketMessage arg) {
            if (arg.Author.IsBot) {
                return FakeTask;
            }
            new Func<Task>(async () => { await AutoResponse.Instance.Respond(arg); }).RunAsyncSave();
            return FakeTask;
        }

        private Task ClientLatencyUpdated(int old, int val) {
            new Action(() => { LastPings.Enqueue(val); }).RunAsyncSave();

            return FakeTask;
        }

        private static Task ClientMessageReceived(SocketMessage arg) {
            new Action(() => {
                MessageInterpreter interpreter = new(arg);
                if (interpreter.Executable) {
                    interpreter.TryExecute();
                }
            }).RunAsyncSave();
            return FakeTask;
        }

        private static Task ClientReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3) {
            new Func<Task>(async () => {
                if (arg3.UserId == Program.Client.BotUserId
                    || arg3.User.Value?.IsBot == true
                    || arg3.Message.IsSpecified && arg3.Message.Value.Author.Id != Program.Client.BotUserId
                    || arg1.HasValue && arg1.Value.Author.Id != Program.Client.BotUserId) {
                    return;
                }
                IUserMessage message;
                try {
                    message = await arg1.GetOrDownloadAsync();
                } catch (Exception e) {
                    LogService.Instance.Log(e.ToString(), LogService.LogType.Main, LogSeverity.Notice);
                    return;
                }

                if (message?.Author?.Id == Program.Client.BotUserId) {
                    await Honor.Instance.HonorUserWithReaction(message, arg3);
                }
            }).RunAsyncSave();

            return FakeTask;
        }

        public IVoiceChannel GetVoiceChannelById(ulong id) => _client.GetChannel(id) as IVoiceChannel;

        public IMessageChannel GetMessageChannelById(ulong id) => _client.GetChannel(id) as IMessageChannel;

        public async Task<RestUser> GetRestUserById(ulong id) => await _client.Rest.GetUserAsync(id);

        public SocketRole GetRoleById(ulong id) => _client.Guilds.SelectMany(guild => guild.Roles).FirstOrDefault(role => role.Id == id);

        public SocketUser GetSocketUserById(ulong id) => _client.GetUser(id);

        public SocketUser GetSocketUserByUsername(string username, string discriminator) => _client.GetUser(username, discriminator);

        private static Task Init() {
            new Action(() => {
                _ = Rememberer.Instance; //initiate Timers
                _ = ServerGroupCalls.Instance; //initiate Running Calls
            }).RunAsyncSave();
            return FakeTask;
        }

        private Task JoinedGuild(SocketGuild arg) {
            new Func<Task>(async () => {
                GuildCountChanged?.Invoke();
                await Messaging.Instance.SendWelcomeMessage(arg);
            }).RunAsyncSave();
            return FakeTask;
        }

        private Task LeftGuild(SocketGuild arg) {
            new Action(() => { GuildCountChanged?.Invoke(); }).RunAsyncSave();
            return FakeTask;
        }

        [SuppressMessage("ReSharper", "InvertIf")]
        public void Start() {
            StateChange();
            UptimeHeartbeatClient heartbeatClient = new();
            Task connector = new(async () => {
                for (int i = 0; ; i++) {
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
                    } catch (Exception e) {
                        LogService.Instance.Log(e.Message, LogService.LogType.Login, LogSeverity.Error);
                    }
                    if (i % 12 == 0) {
                        i = 0;
                        heartbeatClient.Call().RunAsyncSave();
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
            if (user.IsBot) {
                return FakeTask;
            }
            new Func<Task>(async () => {
                //entering
                if (newState.VoiceChannel != null && oldState.VoiceChannel != newState.VoiceChannel) {
                    await ServerGroupCalls.Instance.CheckEntering(user, newState.VoiceChannel);
                }

                //leaving
                if (oldState.VoiceChannel != null && oldState.VoiceChannel != newState.VoiceChannel) {
                    await ServerGroupCalls.Instance.CheckLeaving(user, oldState.VoiceChannel);
                }
            }).RunAsyncSave();
            return FakeTask;
        }

        public class StateChangedData {
            private string _value;

            public static implicit operator string(StateChangedData s) => s._value;

            public static implicit operator StateChangedData(string s) => new() { _value = s };
        }
    }
}