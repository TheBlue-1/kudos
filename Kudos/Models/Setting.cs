#region
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Discord;
using Kudos.Bot;
using Kudos.Exceptions;
using Kudos.Extensions;
#endregion

namespace Kudos.Models {
	public class Setting<T> : Setting {
		private T _setValue;
		public T Default { get; }
		public T Value => IsSet ? SetValue : Default;

		public override bool IsSet { get; protected set; }
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
		public override string StringValue {
			get => Value.ToString();
			
		}
		public override bool SetValueWithString(string value,int parameterIndex = 1) {
			if (Value is IEnumerable) {
				throw new NotImplementedException();
			}
			if (value == null) {
				SetValue = default;
				IsSet = false;
				return false;
			}

			//index 1 because settings get values on index 1
			T newValue = value.ToValue<T>(parameterIndex);
			if (newValue == null) {
				throw new KudosInternalException("value shouldn't be null");
			}
			SetValue = newValue;
			return true;
		}

	

		public override bool AddValueWithString(string value, int valueParameterIndex, string key, int? keyParameterIndex) => throw new NotImplementedException();

		public Setting(T defaultValue) => Default = defaultValue;

		public override event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
