#region
using System;
#endregion

namespace Kudos.Attributes {
	[AttributeUsage(AttributeTargets.Class)]
	public class CommandModule : Attribute {
		public Accessibility Accessibility { get; }
		public string Name { get; }

		public CommandModule(string name, Accessibility accessibility = Accessibility.Open) {
			Name = name;
			Accessibility = accessibility;
		}
	}
}
