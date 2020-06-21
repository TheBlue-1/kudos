#region
using System;
#endregion

namespace Kudos.Attributes {
	[AttributeUsage(AttributeTargets.Class)]
	public class CommandModule : Attribute {
		public string Name { get; }

		public CommandModule(string name) => Name = name;
	}
}
