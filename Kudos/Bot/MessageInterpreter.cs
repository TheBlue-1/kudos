#region
using System.Linq;
using Discord.WebSocket;
#endregion

namespace Kudos.Bot {
	public class MessageInterpreter {
		private const string Prefix = "bot ";
		private string Command { get; }

		public bool Executable { get; } = true;

		private SocketMessage Message { get; }

		private string[] Parameters { get; }

		public MessageInterpreter(SocketMessage message) {
			if (message.Author.IsBot || !message.Content.StartsWith(Prefix)) {
				Executable = false;
				return;
			}
			string[] contentParts = message.Content.Substring(Prefix.Length).Split(' ');
			if (contentParts.Length < 1 || string.IsNullOrEmpty(contentParts[0])) {
				Executable = false;
				return;
			}
			Message = message;
			Command = contentParts[0];
			Parameters = contentParts.Length > 1 ? contentParts.Skip(1).ToArray() : new string[0];
		}

		public void Execute() {
			switch (Command) {
				case "" : break;
				case "hello" :
					Messaging.Instance.Hello(Message.Channel, Message.Author.Username);
					break;
				case "delete" :
					Managing.Instance.Delete(Message.Channel, ParameterAsInt(0));
					break;
				case "help" :
					Messaging.Instance.Help(Message.Channel);
					break;
			}
		}

		private int ParameterAsInt(int index) {
			int value = 0;
			if (Parameters.Length > index) {
				int.TryParse(Parameters[index], out value);
			}
			return value;
		}

		private void Test() { }
	}
}
