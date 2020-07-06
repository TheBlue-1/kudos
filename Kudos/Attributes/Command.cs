#region
using System;
#endregion

namespace Kudos.Attributes {
	[AttributeUsage(AttributeTargets.Method)]
	public class Command : Attribute {
		public string Description { get; }
		public bool Hidden { get; }
		public string Name { get; }

		public Command(string name, string description = "", bool hidden = false) {
			Name = name;
			Description = description;
			Hidden = hidden;
		}
	}
}
