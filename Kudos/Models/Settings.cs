﻿#region
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Discord;
using Kudos.Extensions;
using Kudos.Models.bases;
#endregion

namespace Kudos.Models {
	public class Settings : SettingList, INotifyPropertyChanged {
		private readonly ImmutableDictionary<SettingNames, SettingBase> _settings;

		public SettingBase this[SettingNames name] => _settings[name];

		public Settings() {
			IEnumerable<FieldInfo> fields = GetType()
				.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
				.Where(field => field.FieldType.IsSubclassOf(typeof (SettingBase)));
			Dictionary<SettingNames, SettingBase> settings = fields.Select(field => (SettingBase)field.GetValue(this)).ToDictionary(setting => setting.Name);
			foreach (SettingBase setting in settings.Values) {
				setting.PropertyChanged += SettingChanged;
			}
			_settings = settings.ToImmutableDictionary();
		}

		private Settings(ImmutableDictionary<SettingNames, SettingBase> settings) : this() {
			foreach ((SettingNames key, SettingBase value) in settings) {
				if (value.IsSet) {
					_settings[key].ObjectValue = value.ObjectValue;
				}
			}
		}

		public Settings Merge(Settings userSettings) {
			return new Settings(ImmutableDictionary.CreateRange(userSettings._settings.Select(setting =>
				setting.Value.IsSet ? setting : _settings.First(serverSetting => serverSetting.Key == setting.Key))));
		}

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void SettingChanged(object sender, PropertyChangedEventArgs args) {
			OnPropertyChanged();
		}

		public static EmbedBuilder SettingsAsEmbed() {
			EmbedBuilder builder = new EmbedBuilder().SetDefaults();

			EmbedFieldBuilder commandsField = new EmbedFieldBuilder().WithIsInline(false).WithName("Commands");
			CommandModuleInfo settingsInfo = new CommandModuleInfo(typeof (Bot.Modules.Settings));
			string commandDescription = settingsInfo.Commands.First(command => command.Command.Name == "s").ToString();
			commandsField.WithValue(commandDescription);
			builder.AddField(commandsField);
			EmbedFieldBuilder settingsField = new EmbedFieldBuilder().WithIsInline(false).WithName("Settings");
			string settings = string.Empty;
			Settings defaultSettings = new Settings();
			settings = defaultSettings._settings.Values.Aggregate(settings, (current, defaultSetting) => current + defaultSetting.HelpText + "\n");
			builder.AddField(settingsField.WithValue(settings));
			EmbedFieldBuilder examplesField = new EmbedFieldBuilder().WithIsInline(false).WithName("Examples");
			string examples = "set `prefix` server wide:\n" + "`k!s prefix \"kudos!\" - true`\n";
			examples += "unset (to default) `prefix` personal:\n" + "`k!s prefix`\n";
			examples += "set `autoreact` for messages ending with `haha` to `😂` personal:\n" + "`k!s autoreact \"😂\" \"*haha\"`\n";
			examples += "unset `autoreact` for messages starting with `haha` server wide:\n" + "`k!s autoreact - \"haha*\" true`\n";
			examplesField.WithValue(examples);
			return builder.AddField(examplesField);
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
