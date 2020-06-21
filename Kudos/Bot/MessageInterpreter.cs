#region
using System;
using System.Linq;
using System.Text.RegularExpressions;
using Discord.WebSocket;
using Kudos.Models;
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
			Command = contentParts[0].ToLower();
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
			CommandInfo command = CommandModules.Instance.Modules
				.SelectMany(module => module.Commands.Where(commandInfo => commandInfo.Command.Name == Command))
				.FirstOrDefault();
			if (command == null) {
				return;
			}
			object commandModule = command.Module.Type.GetProperty("Instance")?.GetValue(null);
			if (commandModule == null) {
				throw new Exception("command modules must be singletons");
			}
			CommandParameterInfo[] parameterInfo = command.AllParameter;
			object[] parameters = new object[parameterInfo.Length];
			for (int i = 0; i < parameters.Length; i++) {
				parameters[i] = parameterInfo[i].CommandParameter.FormParameter(parameterInfo[i].ParameterInfo, Parameters, Message);
			}
			command.MethodInfo.Invoke(commandModule, parameters);
		}
	}
}
