#region
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Discord;
using Kudos.Attributes;
#endregion

namespace Kudos.Models {
	public class CommandModuleInfo {
		public EmbedFieldBuilder CommandListAsEmbedField {
			get {
				EmbedFieldBuilder fieldBuilder = new EmbedFieldBuilder().WithName(Module.Name).WithIsInline(false);
				string commands = Commands.Where(command => !command.Command.Hidden).Aggregate("", (current, command) => current + (command + "\n"));
				return string.IsNullOrEmpty(commands) ? null : fieldBuilder.WithValue(commands);
			}
		}

		public string CommandListAsHtml {
			get {
				string htmlList = $@"
<p>
<h3>{Module.Name}</h3>
<table>
";
				htmlList = Commands.Where(command => !command.Command.Hidden).Aggregate(htmlList, (current, commandInfo) => current + commandInfo.ToHtml());
				htmlList += @"
</table>
</p> 
";
				return htmlList;
			}
		}
		public IEnumerable<CommandInfo> Commands { get; }
		public CommandModule Module { get; }
		public Type Type { get; }

		public CommandModuleInfo(Type type) {
			Type = type;
			Module = type.GetCustomAttribute<CommandModule>();
			Commands = Type.GetMethods()
				.Where(method => method.CustomAttributes.Any(attribute => attribute.AttributeType == typeof (Command)))
				.Select(methodInfo => new CommandInfo(methodInfo, this));
		}
	}
}
