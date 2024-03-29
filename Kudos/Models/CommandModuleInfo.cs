﻿#region

using Discord;
using Kudos.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion

namespace Kudos.Models {

    public class CommandModuleInfo {

        public string CommandListAsHtml {
            get {
                string htmlList = $@"
<h3>{Module.Name}</h3>
<table>
";
                htmlList = Commands.Where(command => command.Command.Accessibility != Accessibility.Hidden)
                    .Aggregate(htmlList, (current, commandInfo) => current + commandInfo.ToHtml());
                htmlList += @"
</table>
";
                return htmlList;
            }
        }

        public IEnumerable<CommandInfo> Commands { get; }
        public CommandModule Module { get; }
        public Type Type { get; }

        public CommandModuleInfo(Type type) {
            Type = type;
            Module = type.GetCustomAttribute<CommandModule>();
            Commands = Type.GetMethods()
                .Where(method => method.CustomAttributes.Any(attribute => attribute.AttributeType == typeof(Command)))
                .Select(methodInfo => new CommandInfo(methodInfo, this));
        }

        public EmbedFieldBuilder CommandListAsEmbedField(bool isBotAdmin) {
            if (Module.Accessibility == Accessibility.Hidden && !isBotAdmin) {
                return null;
            }
            EmbedFieldBuilder fieldBuilder = new EmbedFieldBuilder().WithName(Module.Name).WithIsInline(false);
            string commands = Commands.Where(command => command.Command.Accessibility != Accessibility.Hidden || isBotAdmin)
                .Aggregate("", (current, command) => current + (command + "\n"));
            return string.IsNullOrEmpty(commands) ? null : fieldBuilder.WithValue(commands);
        }
    }
}