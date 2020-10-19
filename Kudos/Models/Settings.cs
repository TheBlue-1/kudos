#region
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
		public static string SettingsListAsHtml {
			get {
				string htmlList = @"
<h2>Settings</h2>
<p>
<h3>Commands</h3>
<table>
";
				CommandModuleInfo settingsInfo = new CommandModuleInfo(typeof (Bot.Modules.Settings));
				CommandInfo settingCommand = settingsInfo.Commands.First(command => command.Command.Name == "s");
				htmlList += settingCommand.ToHtml();
				htmlList += @"
</table>
</p> 
";
				htmlList += @"

<p>
<h3>Settings</h3>
<table>
";
				string settings = string.Empty;
				Settings defaultSettings = new Settings();
				settings = defaultSettings._settings.Values.Aggregate(settings, (current, defaultSetting) => current + defaultSetting.HtmlHelpText);
				htmlList += settings;
				htmlList += @"
</table>
</p> 
";
				htmlList += @"

<p>
<h3>Examples</h3>
<table>
";
				htmlList +=
					"<tr><td>set <b>prefix</b> server wide</td><td><b>k!s prefix \"kudos!\" - true</b></td></tr><tr><td>unset(to default) <b> prefix </b> personal</td><td><b> k!s prefix</b></td></tr><tr><td>set <b> autoreact </b> for messages ending with <b> haha </b> to <b>😂</b> personal</td><td><b> k!s autoreact \"😂\" \"*haha\"</b></td></tr><tr><td>unset <b> autoreact </b> for messages starting with <b> haha </b> server wide</td><td><b> k!s autoreact - \"haha*\" true</b></td></tr>";
				htmlList += @"
</table>
</p> 
";
				return htmlList;
			}
		}

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
			return new Settings(ImmutableDictionary.CreateRange(userSettings._settings.Select(settingNamePair => {
				(SettingNames name, SettingBase setting) = settingNamePair;
				KeyValuePair<SettingNames, SettingBase> serverSetting = _settings.First(serverSettingItem => name == serverSettingItem.Key);
				return new KeyValuePair<SettingNames, SettingBase>(name, setting.Merge(serverSetting.Value));
			})));
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
