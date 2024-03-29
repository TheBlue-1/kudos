﻿#region

using Discord;
using Kudos.Attributes;
using Kudos.Exceptions;
using Kudos.Extensions;
using Kudos.Models;
using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace Kudos.Bot {

    public sealed class CommandModules {
        public static CommandModules Instance { get; } = new();

        public IEnumerable<CommandModuleInfo> Modules { get; }

        static CommandModules() {
        }

        private CommandModules() {
            Modules = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.CustomAttributes.Any(attribute => attribute.AttributeType == typeof(CommandModule)))
                .Select(type => new CommandModuleInfo(type));
        }

        public EmbedBuilder CommandListAsEmbed(bool isBotAdmin = false) {
            if (Modules.Count() > 24) {
                throw new KudosInternalException("Too many CommandModules, merge them");
            }
            EmbedBuilder embedBuilder = new EmbedBuilder().SetDefaults()
                .WithTitle("Command List")
                .WithDescription(
                    "While the Bots main feature is a honor/ranking system, Kudos also comes with a bunch of features for server calls and the option for automated tasks and features. More information on [top.gg](https://top.gg/bot/719571683517792286).");

            foreach (CommandModuleInfo module in Modules) {
                EmbedFieldBuilder field = module.CommandListAsEmbedField(isBotAdmin);
                if (field != null) {
                    embedBuilder.AddField(field);
                }
            }
            string types =
                (from value in ParameterType.KnownParameterTypes let valueString = value.ToString() where valueString != string.Empty select value).Aggregate(
                    "`[t|n?(d)]` t is the Type, n is the name, ? tells it's optional and d is the default value\n"
                    + "use `-` to skip a parameter, surround parameters containing spaces with \" like \"long parameter\"",
                    (current, value) => current + ("\n" + value));
            embedBuilder.AddField(new EmbedFieldBuilder().WithName("Types").WithIsInline(false).WithValue(types));
            return embedBuilder;
        }
    }
}