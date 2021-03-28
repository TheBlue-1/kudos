#region
using System;
using System.Collections.Generic;
using Discord;
#endregion

namespace Kudos.Attributes {
	[AttributeUsage(AttributeTargets.Method)]
	public class Command : Attribute {
		public Accessibility Accessibility { get; }
		public string Description { get; }
		public string Name { get; }
		public IEnumerable<GuildPermission> Permissions { get; }

		public Command(string name, string description = "", Accessibility accessibility = Accessibility.Open, GuildPermission[] permissions = null) {
			Name = name;
			Description = description;
			Accessibility = accessibility;
			Permissions = permissions ?? Array.Empty<GuildPermission>();
		}
	}

	public enum Accessibility {
		Open,
		Admin,
		Hidden
	}
}
