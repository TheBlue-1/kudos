#region
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
