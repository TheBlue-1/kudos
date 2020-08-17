#region
using System;
#endregion

namespace Kudos.Attributes {
	[AttributeUsage(AttributeTargets.Method)]
	public class Command : Attribute {
		public Accessibility Accessibility { get; }
		public string Description { get; }
		public string Name { get; }

		public Command(string name, string description = "", Accessibility accessibility = Accessibility.Open) {
			Name = name;
			Description = description;
			Accessibility = accessibility;
		}
	}

	public enum Accessibility {
		Open,
		Admin,
		Hidden
	}
}
