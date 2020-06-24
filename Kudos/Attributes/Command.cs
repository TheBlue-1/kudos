#region
using System;
#endregion

namespace Kudos.Attributes {
	[AttributeUsage(AttributeTargets.Method)]
	public class Command : Attribute {
		public string Description { get; }
		public string Name { get; }

		public Command(string name, string description = "") {
			Name = name;
			Description = description;
		}
	}
}
