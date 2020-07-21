#region
using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Kudos.Attributes;
using Kudos.Exceptions;
using Kudos.Models;
using Kudos.Utils;

// ReSharper disable UnusedMember.Global
#endregion

namespace Kudos.Bot.Modules {
	[CommandModule("Settings")]
	public sealed class Settings {
		public static Settings Instance { get; } = new Settings();

		static Settings() { }

		private Settings() { }

		[Command("s", "change settings")]
		public async Task ChangeSetting([CommandParameter] ISocketMessageChannel channel, [CommandParameter(0)] Word setting,
			[CommandParameter(1, null)] string value, [CommandParameter(2, null)] string key, [CommandParameter(3, false)] bool forServer,
			[CommandParameter] SocketUser author) {
			Models.Settings settings = EditableSettings(forServer, channel, author);
			if (!Enum.TryParse(setting.Value, true, out SettingNames settingName)) {
				throw new KudosArgumentException($"Setting '{setting}' doesn't exist");
			}
			if (settings[settingName].AddOrSetValue(value, 1, key, 2)) {
				await Messaging.Instance.SendMessage(channel, $"'{setting}' set to '{value}' {(forServer ? "server wide" : "personal")}");
			} else {
				await Messaging.Instance.SendMessage(channel, $"'{setting}' unset {(forServer ? "server wide" : "personal")}");
			}
		}

		private static Models.Settings EditableSettings(bool forServer, ISocketMessageChannel channel, SocketUser author) {
			if (!forServer) {
				return SettingsManager.Instance.SettingsFor(author.Id, create: true);
			}
			if (channel is SocketGuildChannel guildChannel) {
				return SettingsManager.Instance.SettingsFor(guildId: guildChannel.Guild.Id, create: true);
			}

			throw new KudosArgumentException("Can not set server settings in pms");
		}

		// ReSharper disable once StringLiteralTypo
		[Command("autoreact", "set automatic reactions to messages")]
		public async Task SetAutoReact([CommandParameter] ISocketMessageChannel channel, [CommandParameter(0)] string message,
			[CommandParameter(1, null)] IEmote emoji, [CommandParameter(2, false)] bool forServer, [CommandParameter] SocketUser author) {
			Models.Settings settings = EditableSettings(forServer, channel, author);
			settings[SettingNames.AutoReact].AsSetting(out Setting<ImmutableDictionary<string, string>> autoReactSetting);
			autoReactSetting.SetValue = autoReactSetting.Value.SetItem(message, emoji.ToString());
			await Messaging.Instance.SendMessage(channel,
				emoji.ToString() == null
					? $"unset for `{message}` {(forServer ? "server wide" : "personal")}"
					: $"set `{emoji}` for `{message}` {(forServer ? "server wide" : "personal")}");
		}

		[Command("prefix", "changes the prefix")]
		public async Task SetPrefix([CommandParameter] ISocketMessageChannel channel, [CommandParameter(0)] string prefix,
			[CommandParameter(1, false)] bool forServer, [CommandParameter] SocketUser author) {
			EditableSettings(forServer, channel, author)[SettingNames.Prefix].AsSetting(out Setting<string> prefixSetting);
			prefixSetting.SetValue = prefix;
			await Messaging.Instance.SendMessage(channel, $"prefix set to `{prefix}` {(forServer ? "server wide" : "personal")}");
		}
	}
}
