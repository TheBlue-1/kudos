#region

using Discord;
using System;
using System.Collections.Generic;

#endregion

namespace Kudos.Attributes {

    [AttributeUsage(AttributeTargets.Class)]
    public class CommandModule : Attribute {
        public Accessibility Accessibility { get; }
        public string Name { get; }
        public IEnumerable<GuildPermission> Permissions { get; }

        public CommandModule(string name, Accessibility accessibility = Accessibility.Open, GuildPermission[] permissions = null) {
            Name = name;
            Accessibility = accessibility;
            Permissions = permissions ?? Array.Empty<GuildPermission>();
        }
    }
}