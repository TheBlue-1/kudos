#region
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
#endregion

namespace Kudos.Bot {
	public class Client {
		private readonly DiscordSocketClient _client;

		public string State => _client.Status.ToString();

		public Client(string token) {
			_client = new DiscordSocketClient();
			_client.SetGameAsync("with the 'bot help' command");
			_client.MessageReceived += ClientMessageReceived;
			_client.ReactionAdded += ClientReactionAdded;
			Start(token);
		}

		private async Task ClientMessageReceived(SocketMessage arg) {
			await Task.Run(() => {
				MessageInterpreter interpreter = new MessageInterpreter(arg);
				if (interpreter.Executable) {
					interpreter.Execute();
				}
			});
		}

		private async Task ClientReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3) {
			await Task.Run(() => {
				IMessage message = arg1.HasValue ? arg1.Value : arg3.Message.GetValueOrDefault() ?? arg2.GetMessageAsync(arg1.Id).GetAwaiter().GetResult();
				message.AddReactionAsync(arg3.Emote);
			});
		}

		public async Task<RestUser> GetRestUserById(ulong id) => await _client.Rest.GetUserAsync(id);

		private async void Start(string token) {
			await _client.LoginAsync(TokenType.Bot, token);
			await _client.StartAsync();
		}
	}
}
