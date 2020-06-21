#region
using System;
using System.Collections.Generic;
using System.Linq;
using Kudos.Attributes;
using Kudos.Models;
#endregion

namespace Kudos.Bot {
	public sealed class CommandModules {
		public static CommandModules Instance { get; } = new CommandModules();

		public IEnumerable<CommandModuleInfo> Modules { get; }

		static CommandModules() { }

		private CommandModules() {
			Modules = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(assembly => assembly.GetTypes())
				.Where(type => type.CustomAttributes.Any(attribute => attribute.AttributeType == typeof (CommandModule)))
				.Select(type => new CommandModuleInfo(type));
		}

		public override string ToString() {
			string info = Modules.Aggregate("Kudos Commands:\n", (current, module) => current + module);
			info += @"Types:
```
[user]      mention with @
            if not possible use full username like '@Kudos#9294'
            (you can also get this by copying a mention)
[x?]	    optional
```
version: "
				+ Program.Version;
			return info;
		}
	}
}
