#region
using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Kudos.Attributes;
using Kudos.Exceptions;
using Kudos.Extensions;
using Kudos.Models;
#endregion

namespace Kudos.Bot {
	public sealed class CommandModules {
		public EmbedBuilder CommandListAsEmbed {
			get {
				if (Modules.Count() > 24) {
					throw new KudosInternalException("Too many CommandModules, merge them");
				}
				EmbedBuilder embedBuilder = new EmbedBuilder().SetDefaults()
					.WithTitle("Command List")
					.WithDescription("Kudos is a bot with a honor system and many other features.");
				foreach (CommandModuleInfo module in Modules) {
					embedBuilder.AddField(module.CommandListAsEmbedField);
				}
				string types = ParameterType.ParameterTypes.Values.Aggregate("`[x|y?]` x is the Type, y is the name, ? tells it's optional",
					(current, type) => current + ("\n" + type));
				embedBuilder.AddField(new EmbedFieldBuilder().WithName("Types").WithIsInline(false).WithValue(types));
				return embedBuilder;
			}
		}
		public static CommandModules Instance { get; } = new CommandModules();

		public IEnumerable<CommandModuleInfo> Modules { get; }

		static CommandModules() { }

		private CommandModules() {
			Modules = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(assembly => assembly.GetTypes())
				.Where(type => type.CustomAttributes.Any(attribute => attribute.AttributeType == typeof (CommandModule)))
				.Select(type => new CommandModuleInfo(type));
		}
	}
}
