#region
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
#endregion

namespace Kudos.Models {
	public abstract class Setting : INotifyPropertyChanged {
		public abstract bool IsSet { get; protected set; }
		public abstract string StringValue { get; }

		public abstract bool SetValueWithString(string value, int parameterIndex=1);
		public abstract bool AddValueWithString(string value, int valueParameterIndex=1,string key=null, int? keyParameterIndex=null);


		public bool AddOrSetValue(string value, int valueParameterIndex = 1, string key = null, int? keyParameterIndex = null) => this is Setting<IEnumerable> ? AddValueWithString(value, valueParameterIndex, key, keyParameterIndex) : SetValueWithString(value, valueParameterIndex);

		public void AsSetting<T>(out T value)
			where T : Setting {
			value = this as T;
		}

		public void Value<T>(out T value) {
			value = this is Setting<T> setting ? setting.Value : default;
		}

		public abstract event PropertyChangedEventHandler PropertyChanged;
	}
}
