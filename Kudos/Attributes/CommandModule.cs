#region
using System;
#endregion

namespace Kudos.Attributes {
	[AttributeUsage(AttributeTargets.Class)]
	public class CommandModule : Attribute {
		public bool Hidden { get; }
		public string Name { get; }

		public CommandModule(string name, bool hidden = false) {
			Name = name;
			Hidden = hidden;
		}
	}
}
