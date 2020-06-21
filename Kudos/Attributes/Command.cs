#region
using System;
#endregion

namespace Kudos.Attributes {
	[AttributeUsage(AttributeTargets.Method)]
	public class Command : Attribute {
		public string Name { get; }

		public Command(string name) => Name = name;
	}
}
