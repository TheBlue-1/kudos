#region
using System.ComponentModel;
using System.Runtime.CompilerServices;
#endregion

namespace Kudos.Models {
	public class Settings : INotifyPropertyChanged {
		public Setting<string> Prefix { get; }

		public Settings() => Prefix = new Setting<string>("bot ", SettingChanged);

		private Settings(Setting<string> prefix) => Prefix = prefix;

		public Settings Merge(Settings userSettings) => new Settings(userSettings.Prefix.IsSet ? userSettings.Prefix : Prefix);

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void SettingChanged(object sender, PropertyChangedEventArgs args) {
			OnPropertyChanged();
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
