#region
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
#endregion

namespace Kudos.Bot {
	public class Client {
		//TODO sleep remember
		private readonly DiscordSocketClient _client;

		public string State => _client.Status.ToString();

		public Client(string token) {
			_client = new DiscordSocketClient();
			_client.SetGameAsync($"with the '{MessageInterpreter.Prefix}help' command");
			_client.MessageReceived += ClientMessageReceived;
			_client.ReactionAdded += ClientReactionAdded;
			Start(token);
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
		public SocketUser GetSocketUserByUsername(string username, string discriminator) => _client.GetUser(username, discriminator);

		private async void Start(string token) {
			await _client.LoginAsync(TokenType.Bot, token);
			await _client.StartAsync();
		}
	}
}
