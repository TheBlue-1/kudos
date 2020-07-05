#region
using System.ComponentModel;
using System.Runtime.CompilerServices;
#endregion

namespace Kudos.Models {
	public class Setting<T> : INotifyPropertyChanged {
		private T _setValue;
		public T Default { get; }
		public T Value => IsSet ? SetValue : Default;

		public bool IsSet { get; private set; }
		public T SetValue {
			get => _setValue;
			set {
				if (value == null) {
					return;
				}
				_setValue = value;
				IsSet = true;
				OnPropertyChanged();
			}
		}

		public Setting(T defaultValue, PropertyChangedEventHandler changedEventHandler) {
			Default = defaultValue;
			PropertyChanged += changedEventHandler;
		}

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
