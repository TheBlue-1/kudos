#region
using System;
using System.Linq;
using System.Text.RegularExpressions;
using Discord.WebSocket;
using Kudos.Exceptions;
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
			string[] contentParts = Regex.Split(message.Content.Substring(Prefix.Length), " @| (?<! @[^#]*?)");
			if (contentParts.Length < 1 || string.IsNullOrEmpty(contentParts[0])) {
				Executable = false;
				return;
			}
			Message = message;
			Command = contentParts[0];
			Parameters = contentParts.Skip(1).ToArray();
		}

		public void TryExecute() {
			try {
				Execute();
			}
			catch (Exception e) {
				new ExceptionHandler(e, Message.Channel).Handle();
			}
		}

		public void Execute() {
			switch (Command) {
				case "" : break;
				case "hello" :
					Messaging.Instance.Hello(Message.Channel, Message.Author);
					break;
				case "delete" :
					Managing.Instance.Delete(Message.Channel, ParameterAsInt(0, true, 1));
					break;
				case "help" :
					Messaging.Instance.Help(Message.Channel);
					break;
				case "balance" :
					Honor.Instance.SendHonorBalance(ParameterAsUser(0, true, Message.Author), Message.Channel);
					break;
				case "honor" :
					Honor.Instance.HonorUser(ParameterAsUser(1, false), Message.Author, ParameterAsInt(0, true, 1), Message.Channel);
					break;
				case "dishonor" :
					Honor.Instance.DishonorUser(ParameterAsUser(1, false), Message.Author, ParameterAsInt(0, true, 1), Message.Channel);
					break;
				case "question" :
					AnonymousQuestion.Instance.AskAnonymous(ParametersFrom(1, false), ParameterAsUser(0, false), Message.Author, Message.Channel);
					break;
				case "answer" :
					AnonymousQuestion.Instance.Answer(ParameterAsULong(0, false), ParametersFrom(1, false), Message.Author, Message.Channel);
					break;
			}
		}

		private ulong ParameterAsULong(int index, bool optional = true, ulong defaultValue = 0) {
			if (Parameters.Length > index && ulong.TryParse(Parameters[index], out ulong value)) {
				return value;
			}
			if (optional) {
				value = defaultValue;
			} else {
				throw new KudosArgumentException($"Parameter {index + 1} must be a number (ulong)");
			}
			return value;
		}

		private int ParameterAsInt(int index, bool optional = true, int defaultValue = 0) {
			if (Parameters.Length > index && int.TryParse(Parameters[index], out int value)) {
				return value;
			}
			if (optional) {
				value = defaultValue;
			} else {
				throw new KudosArgumentException($"Parameter {index + 1} must be a number (int)");
			}
			return value;
		}

		private SocketUser ParameterAsUser(int index, bool optional = true, SocketUser defaultValue = null) {
			SocketUser user = Message.MentionedUsers.FirstOrDefault();
			if (user != null) {
				return user;
			}
			if (Parameters.Length > index) {
				string[] userData = Parameters[index].Split("#");
				if (userData.Length == 2) {
					user = Program.Client.GetSocketUserByUsername(userData[0], userData[1]);
				}
				if (user != null) {
					return user;
				}
			}
			if (!optional) {
				throw new KudosArgumentException($"Parameter {index + 1} must be a user (described in help)");
			}

			user = defaultValue;
			return user;
		}

		private string ParametersFrom(int index, bool optional = true, string defaultValue = "") {
			if (Parameters.Length > index) {
				return string.Join(" ", Parameters.Skip(index));
			}
			if (optional) {
				return defaultValue;
			}
			throw new KudosArgumentException($"Parameter {index + 1} must be a text");
		}
	}
}
