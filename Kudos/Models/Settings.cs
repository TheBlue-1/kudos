#region
using System.Collections.Immutable;
using System.ComponentModel;
using System.Runtime.CompilerServices;
#endregion

namespace Kudos.Models {
	public class Settings : INotifyPropertyChanged {
		public Setting<ImmutableDictionary<string, string>> AutoReact { get; }
		public Setting<string> Prefix { get; }

		public Settings() {
			Prefix = new Setting<string>("k!", SettingChanged);
			AutoReact = new Setting<ImmutableDictionary<string, string>>(ImmutableDictionary<string, string>.Empty, SettingChanged);
		}

		private Settings(Setting<string> prefix, Setting<ImmutableDictionary<string, string>> autoReact) {
			Prefix = prefix;
			AutoReact = autoReact;
		}

		public Settings Merge(Settings userSettings) => new Settings(userSettings.Prefix.IsSet ? userSettings.Prefix : Prefix,
			userSettings.AutoReact.IsSet ? userSettings.AutoReact : AutoReact);

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void SettingChanged(object sender, PropertyChangedEventArgs args) {
			OnPropertyChanged();
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
