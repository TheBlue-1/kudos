#region
using System.Collections.Generic;
using System.ComponentModel;
using Kudos.Extensions;
using Kudos.Models;
#endregion

namespace Kudos.Utils {
	public class SettingsDictionary : AsyncThreadsafeFileSyncedDictionary<ulong, Settings> {
		public override Settings this[ulong key] {
			get => base[key];
			set {
				if (ContainsKey(key)) {
					this[key].PropertyChanged -= SettingsChanged;
				}
				value.PropertyChanged += SettingsChanged;
				base[key] = value;
			}
		}

		public SettingsDictionary(string fileName) : base(fileName) {
			RunLocked(() => {
				// ReSharper disable once AssignmentIsFullyDiscarded
				_ = DictionaryImplementation; //make sure data is loaded

				foreach (Settings settings in DictionaryImplementation.Values) {
					settings.PropertyChanged += SettingsChanged;
				}
			});
		}

		public override void Add(KeyValuePair<ulong, Settings> item) {
			item.Value.PropertyChanged += SettingsChanged;
			base.Add(item);
		}

		public override void Add(ulong key, Settings value) {
			value.PropertyChanged += SettingsChanged;
			base.Add(key, value);
		}

		public override void Clear() {
			RunLocked(() => {
				foreach (Settings settings in DictionaryImplementation.Values) {
					settings.PropertyChanged -= SettingsChanged;
				}
			});
			base.Clear();
		}

		public override bool Remove(KeyValuePair<ulong, Settings> item) {
			if (ContainsKey(item.Key)) {
				item.Value.PropertyChanged -= SettingsChanged;
			}
			return base.Remove(item);
		}

		public override bool Remove(ulong key) {
			if (ContainsKey(key)) {
				this[key].PropertyChanged -= SettingsChanged;
			}
			return base.Remove(key);
		}

		private void SettingsChanged(object sender, PropertyChangedEventArgs args) {
			SaveDictionary().RunAsyncSave();
		}
	}
}
