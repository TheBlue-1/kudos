#region
using System.Collections.Immutable;
using System.ComponentModel;
using System.Runtime.CompilerServices;
#endregion

namespace Kudos.Models {
	public class Settings : INotifyPropertyChanged {
		public Setting<ImmutableDictionary<string, string>> AutoImage { get; }
		public Setting<ImmutableDictionary<string, string>> AutoMessage { get; }
		public Setting<ImmutableDictionary<string, string>> AutoReact { get; }
		public Setting<bool> AutoResponses { get; }
		public Setting<string> Prefix { get; }

		

		public Settings() {
			Prefix = new Setting<string>("k!", SettingChanged);
			AutoResponses = new Setting<bool>(true, SettingChanged);
			AutoReact = new Setting<ImmutableDictionary<string, string>>(ImmutableDictionary<string, string>.Empty, SettingChanged);
			AutoImage = new Setting<ImmutableDictionary<string, string>>(ImmutableDictionary<string, string>.Empty, SettingChanged);
			AutoMessage = new Setting<ImmutableDictionary<string, string>>(ImmutableDictionary<string, string>.Empty, SettingChanged);
		}

		private Settings(Setting<string> prefix, Setting<ImmutableDictionary<string, string>> autoReact, Setting<ImmutableDictionary<string, string>> autoImage,
			Setting<ImmutableDictionary<string, string>> autoMessage, Setting<bool> autoResponses) {
			Prefix = prefix;
			AutoReact = autoReact;
			AutoImage = autoImage;
			AutoMessage = autoMessage;
			AutoResponses = autoResponses;
		}

		public Settings Merge(Settings userSettings) => new Settings(userSettings.Prefix.IsSet ? userSettings.Prefix : Prefix,
			userSettings.AutoReact.IsSet ? userSettings.AutoReact : AutoReact, userSettings.AutoImage.IsSet ? userSettings.AutoImage : AutoImage,
			userSettings.AutoMessage.IsSet ? userSettings.AutoMessage : AutoMessage,
			userSettings.AutoResponses.IsSet ? userSettings.AutoResponses : AutoResponses);

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void SettingChanged(object sender, PropertyChangedEventArgs args) {
			OnPropertyChanged();
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
