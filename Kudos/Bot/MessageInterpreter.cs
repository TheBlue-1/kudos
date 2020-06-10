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
					Honor.Instance.SendHonorBalance(Message.MentionedUsers.FirstOrDefault(), Message.Channel);
					break;
				case "honor" :
					Honor.Instance.HonorUser(Message.MentionedUsers.FirstOrDefault(), Message.Author, ParameterAsInt(0), Message.Channel);
					break;
				case "dishonor" :
					Honor.Instance.DishonorUser(Message.MentionedUsers.FirstOrDefault(), Message.Author, ParameterAsInt(0), Message.Channel);
					break;
				case "question" :
					AnonymousQuestion.Instance.AskAnonymous(ParametersFrom(1), Message.MentionedUsers.FirstOrDefault(), Message.Author);
					break;
				case "answer" :
					AnonymousQuestion.Instance.Answer(ParameterAsULong(0), ParametersFrom(1), Message.Author);
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

		private string ParametersFrom(int index) => string.Join(" ", Parameters.Skip(index));
	}
}
