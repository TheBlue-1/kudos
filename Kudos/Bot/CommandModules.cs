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
		public static CommandModules Instance { get; } = new CommandModules();

		public string LongDescription {
			get {
				string longDescription = @"
<h1>Kudos</h1>
<h4>A bot with a honor system and many other features</h4>
";
				longDescription += @"
<h2>Features</h2>
<p>
<ul>
";
				longDescription = Modules.Where(module => !module.Module.Hidden)
					.Aggregate(longDescription, (current, module) => current + $@"<li>{module.Module.Name}</li>");
				longDescription += @"
</ul>
</p>
";
				longDescription += @"
<h2>Commands</h2>
";

				longDescription = Modules.Where(module => !module.Module.Hidden)
					.Aggregate(longDescription, (current, module) => current + module.CommandListAsHtml);

				longDescription += @"
<h2>Types</h2>
<h4>[x|y?] ... x is the Type, y is the name, ? tells it's optional</h4>
<p>
<table>
";
				longDescription = ParameterType.ParameterTypes.Values.Aggregate(longDescription, (current, parameterType) => current + parameterType.ToHtml());
				longDescription += @"
</table>
</p> 
";
				return longDescription;
			}
		}

		public IEnumerable<CommandModuleInfo> Modules { get; }

		static CommandModules() { }

		private CommandModules() {
			Modules = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(assembly => assembly.GetTypes())
				.Where(type => type.CustomAttributes.Any(attribute => attribute.AttributeType == typeof (CommandModule)))
				.Select(type => new CommandModuleInfo(type));
		}

		public EmbedBuilder CommandListAsEmbed(bool isBotAdmin = false) {
			if (Modules.Count() > 24) {
				throw new KudosInternalException("Too many CommandModules, merge them");
			}
			EmbedBuilder embedBuilder = new EmbedBuilder().SetDefaults()
				.WithTitle("Command List")
				.WithDescription("Kudos is a bot with a honor system and many other features.");
			foreach (CommandModuleInfo module in Modules) {
				EmbedFieldBuilder field = module.CommandListAsEmbedField(isBotAdmin);
				if (field != null) {
					embedBuilder.AddField(field);
				}
			}
			string types =
				(from value in ParameterType.ParameterTypes.Values let valueString = value.ToString() where valueString != string.Empty select value).Aggregate(
					"`[x|y?]` x is the Type, y is the name, ? tells it's optional", (current, value) => current + ("\n" + value));
			embedBuilder.AddField(new EmbedFieldBuilder().WithName("Types").WithIsInline(false).WithValue(types));
			return embedBuilder;
		}
	}
}
