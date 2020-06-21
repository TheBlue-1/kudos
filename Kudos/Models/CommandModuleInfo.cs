#region
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kudos.Attributes;
#endregion

namespace Kudos.Models {
	public class CommandModuleInfo {
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

		public override string ToString() {
			string info = Module.Name + ":\n```";
			info = Commands.Aggregate(info, (current, command) => current + command);
			info += "```";
			return info;
		}
	}
}
