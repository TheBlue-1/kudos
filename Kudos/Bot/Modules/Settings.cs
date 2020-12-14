#region
using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Kudos.Attributes;
using Kudos.Exceptions;
using Kudos.Extensions;
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

		[Command("gs", "change guild settings", Accessibility.Admin)]
		public async Task ChangeGuildSetting([CommandParameter] ISocketMessageChannel channel, [CommandParameter(0)] Word setting,
			[CommandParameter(1, null)] string value, [CommandParameter(2, null)] string key,
			[CommandParameter] SocketUser author)
		{
			await ChangeSetting(channel, setting, value, key, true, author);
		}
		[Command("s", "change settings")]
		public async Task ChangePersonalSetting([CommandParameter] ISocketMessageChannel channel, [CommandParameter(0)] Word setting,
			[CommandParameter(1, null)] string value, [CommandParameter(2, null)] string key,
			[CommandParameter] SocketUser author)
		{
			await ChangeSetting(channel, setting, value, key, false, author);
		}
		public async Task ChangeSetting( ISocketMessageChannel channel, Word setting,
			string value,  string key, bool forServer,
			 SocketUser author)
		{
			Models.Settings settings = EditableSettings(forServer, channel, author);
			if (!Enum.TryParse(setting, true, out SettingNames settingName))
			{
				throw new KudosArgumentException($"Setting `{setting}` doesn't exist");
			}
			if (settings[settingName].AddOrSetValue(value, 1, key, 2))
			{
				await Messaging.Instance.SendMessage(channel,
					$"`{(key != null ? $"{setting} - {key}" : setting.ToString())}` set to `{value}` {(forServer ? "server wide" : "personal")}");
			}
			else
			{
				await Messaging.Instance.SendMessage(channel,
					$"`{(key != null ? $"{setting} - {key}" : setting.ToString())}` unset {(forServer ? "server wide" : "personal")}");
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

		[Command("settings", "shows all settings")]
		public async Task SendSettingList([CommandParameter] ISocketMessageChannel channel) {
			await Messaging.Instance.SendEmbed(channel, Models.Settings.SettingsAsEmbed());
		}

		[Command("svalue", "shows the current value of the setting (this shows the used value (user settings override server settings))")]
		public async Task SendSettingsValue([CommandParameter] ISocketMessageChannel channel, [CommandParameter(0)] Word setting,
			[CommandParameter] SocketMessage message) {
			if (!Enum.TryParse(setting, true, out SettingNames settingName)) {
				throw new KudosArgumentException($"Setting `{setting}` doesn't exist");
			}
			string settingsValue = "";
			object settingValueObject = message.Settings()[settingName].ObjectValue;
			switch (settingValueObject) {
				case IDictionary dictionary :
					settingsValue = dictionary.ToDictionary()
						.Aggregate(settingsValue, (current, pair) => current + $"key: `{pair.Key}` value: `{pair.Value}` \n");
					break;
				case ICollection collection :
					settingsValue = collection.ToCollection().Aggregate(settingsValue, (current, o) => current + $"`{o}`\n");
					break;
				case null :
					settingsValue += "value: not set";
					break;
				default :
					settingsValue += $"value: `{settingValueObject}`";
					break;
			}
			await Messaging.Instance.SendMessage(channel, settingsValue);
		}
	}
}
