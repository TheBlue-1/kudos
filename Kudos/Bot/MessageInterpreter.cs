#region
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.WebSocket;
using Kudos.Attributes;
using Kudos.DatabaseModels;
using Kudos.Exceptions;
using Kudos.Extensions;
using Kudos.Models;
using Kudos.Utils;
#endregion

namespace Kudos.Bot {
	public class MessageInterpreter {
		private string Command { get; }

		public bool Executable { get; } = true;

		private SocketMessage Message { get; }

		private string[] Parameters { get; }

		public MessageInterpreter(SocketMessage message) {
			message.Settings()[SettingNames.Prefix].Value(out string prefix);
			if (message.Author.IsBot || !message.Content.StartsWith(prefix)) {
				Executable = false;
				return;
			}
			string[] contentParts = Regex.Split(message.Content.Substring(prefix.Length), "(?:\\s+)(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
			if (contentParts.Length < 1 || string.IsNullOrEmpty(contentParts[0])) {
				Executable = false;
				return;
			}
			if (DatabaseSyncedList.Instance<BanData>().FirstOrDefault(b => b.UserId == message.Author.Id) != null) {
				Executable = false;
			}
			Message = message;
			Command = contentParts[0].ToLower();
			Parameters = contentParts.Skip(1).ToArray();
			for (int i = 0; i < Parameters.Length; i++) {
				if (Parameters[i].StartsWith('"') && Parameters[i].EndsWith('"')) {
					Parameters[i] = Parameters[i].Substring(1, Parameters[i].Length - 2);
				}
			}
			FileService.Instance.Log($"{message.Author.Id} called {Command} with [{string.Join("];[", Parameters)}]", "access-");
		}

		public void Execute() {
			bool isBotAdmin = Message.Author.IsBotAdmin();
			CommandInfo command = CommandModules.Instance.Modules.Where(module => module.Module.Accessibility != Accessibility.Hidden || isBotAdmin)
				.SelectMany(module => module.Commands.Where(commandInfo =>
					(commandInfo.Command.Accessibility != Accessibility.Hidden || isBotAdmin) && commandInfo.Command.Name == Command))
				.FirstOrDefault();
			if (command == null) {
				throw new KudosArgumentException($"Command '{Command}' doesn't exist!");
			}

			if (command.Module.Module.Accessibility == Accessibility.Admin || command.Command.Accessibility == Accessibility.Admin) {
				if (Message.Author is SocketGuildUser guildUser) {
					if (!guildUser.IsGuildAdmin()) {
						throw new KudosUnauthorizedException("You must be server administrator to use this feature!");
					}
				}
			}

			object commandModule = command.Module.Type.GetProperty("Instance")?.GetValue(null);
			if (commandModule == null) {
				throw new Exception("command modules must be singletons");
			}

			if (command.Parameter.Last().ParameterInfo.ParameterType == typeof (string)) {
				int count = command.Parameter.Length;
				for (int i = count; i < Parameters.Length; i++) {
					Parameters[count - 1] += " " + Parameters[i];
				}
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
				new ExceptionHandler(e, Message.Channel).Handle(true);
			}
		}
	}
}
