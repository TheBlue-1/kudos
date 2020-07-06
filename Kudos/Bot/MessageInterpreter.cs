#region
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.WebSocket;
using Kudos.Exceptions;
using Kudos.Extensions;
using Kudos.Models;
#endregion

namespace Kudos.Bot {
	public class MessageInterpreter {
		private string Command { get; }

		public bool Executable { get; } = true;

		private SocketMessage Message { get; }

		private string[] Parameters { get; }

		public MessageInterpreter(SocketMessage message) {
			string prefix = message.Settings().Prefix.Value;
			if (message.Author.IsBot || !message.Content.StartsWith(prefix)) {
				Executable = false;
				return;
			}
			string[] contentParts = Regex.Split(message.Content.Substring(prefix.Length), "(?: @| (?<! @[^#]*?))(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
			if (contentParts.Length < 1 || string.IsNullOrEmpty(contentParts[0])) {
				Executable = false;
				return;
			}
			Message = message;
			Command = contentParts[0].ToLower();
			Parameters = contentParts.Skip(1).ToArray();
		}

		public void Execute() {
			bool isBotAdmin = Message.Author.IsBotAdmin();
			CommandInfo command = CommandModules.Instance.Modules.Where(module => !module.Module.Hidden || isBotAdmin)
				.SelectMany(module => module.Commands.Where(commandInfo => (!commandInfo.Command.Hidden || isBotAdmin) && commandInfo.Command.Name == Command))
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
			if (command.MethodInfo.GetCustomAttribute<AsyncStateMachineAttribute>() != null) {
				if (command.MethodInfo.ReturnType == typeof (void)) {
					throw new KudosInternalException("async command methods must return Task");
				}

				Task task = (Task)command.MethodInfo.Invoke(commandModule, parameters);
				task?.Wait(new TimeSpan(0, 1, 0));
				return;
			}
			command.MethodInfo.Invoke(commandModule, parameters);
		}

		public void TryExecute() {
			try {
				Execute();
			}
			catch (Exception e) {
				new ExceptionHandler(e, Message.Channel).Handle();
			}
		}
	}
}
