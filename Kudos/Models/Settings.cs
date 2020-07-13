#region
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Discord;
#endregion

namespace Kudos.Models {
	public class Settings : INotifyPropertyChanged {
		private readonly ImmutableDictionary<SettingNames, Setting> _settings;

		public Setting this[SettingNames name] => _settings[name];

		public Settings() : this(ImmutableDictionary.CreateRange(new[] {
			new KeyValuePair<SettingNames, Setting>(SettingNames.Prefix, new Setting<string>("k!")),
			new KeyValuePair<SettingNames, Setting>(SettingNames.AutoResponses, new Setting<bool>(true)),
			new KeyValuePair<SettingNames, Setting>(SettingNames.AutoReact,
				new DictionarySetting<string, IEmote>(ImmutableDictionary<string, IEmote>.Empty)),
			new KeyValuePair<SettingNames, Setting>(SettingNames.AutoImage,
				new DictionarySetting<string, string>(ImmutableDictionary<string, string>.Empty)),
			new KeyValuePair<SettingNames, Setting>(SettingNames.AutoMessage,
				new DictionarySetting<string, string>(ImmutableDictionary<string, string>.Empty))
		})) { }

		private Settings(ImmutableDictionary<SettingNames, Setting> settings) {
			_settings = settings;
			foreach (Setting setting in settings.Values) {
				setting.PropertyChanged += SettingChanged;
			}
		}

		public Setting<T> GetSetting<T>(SettingNames name) => (Setting<T>)_settings[name];

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

		public event PropertyChangedEventHandler PropertyChanged;
	}

	public enum SettingNames {
		Prefix,
		AutoResponses,
		AutoReact,
		AutoImage,
		AutoMessage
	}
}
