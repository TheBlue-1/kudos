#region
using System.Linq;
using Discord.WebSocket;
#endregion

namespace Kudos.Bot {
	public class MessageInterpreter {
	#if DEBUG
		public const string Prefix = "test ";
	#else
		public const string Prefix = "bot ";
	#endif
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
					Messaging.Instance.Hello(Message.Channel, Message.Author);
					break;
				case "delete" :
					Managing.Instance.Delete(Message.Channel, ParameterAsInt(0));
					break;
				case "help" :
					Messaging.Instance.Help(Message.Channel);
					break;
				case "balance" :
					Honor.Instance.SendHonorBalance(ParameterAsUser(0), Message.Channel);
					break;
				case "honor" :
					Honor.Instance.HonorUser(ParameterAsUser(1), Message.Author, ParameterAsInt(0), Message.Channel);
					break;
				case "dishonor" :
					Honor.Instance.DishonorUser(ParameterAsUser(1), Message.Author, ParameterAsInt(0), Message.Channel);
					break;
				case "question" :
					AnonymousQuestion.Instance.AskAnonymous(ParametersFrom(1), ParameterAsUser(0), Message.Author, Message.Channel);
					break;
				case "answer" :
					AnonymousQuestion.Instance.Answer(ParameterAsULong(0), ParametersFrom(1), Message.Author, Message.Channel);
					break;
			}
		}

		private ulong ParameterAsULong(int index) {
			ulong value = 0;
			if (Parameters.Length > index) {
				ulong.TryParse(Parameters[index], out value);
			}
			return value;
		}

		private int ParameterAsInt(int index) {
			int value = 0;
			if (Parameters.Length > index) {
				int.TryParse(Parameters[index], out value);
			}
			return value;
		}

		private SocketUser ParameterAsUser(int index) {
			SocketUser user = Message.MentionedUsers.FirstOrDefault();
			if (user == null) {
				string[] userData = Parameters[index].Split("#");
				if (userData.Length != 2) {
					return null;
				}

				return Program.Client.GetSocketUserByUsername(userData[0], userData[1]);
			}
			return user;
		}

		private string ParametersFrom(int index) => string.Join(" ", Parameters.Skip(index));
	}
}
