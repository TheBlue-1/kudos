#region
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Kudos.Bot;
using Kudos.Exceptions;
using Kudos.Extensions;
using Kudos.Models.bases;
using Newtonsoft.Json;
#endregion

namespace Kudos.Models {
	public class Setting<T> : SettingBase, ISetting {
		private T _setValue;
		[JsonIgnore]
		public T Default { get; }

		public override string HelpText {
			get {
				string info = $"[{ParameterType.FromType(typeof (T)).Character}|value]";
				string text = $"`{Name.ToString().ToLower()} {info}` default: `{Default}` {Description}";
				return text;
			}
		}
		[JsonIgnore]
		public override string StringValue => Value.ToString();
		[JsonIgnore]
		public T Value => IsSet ? SetValue : Default;

		[JsonProperty(Order = 2)]
		public override bool IsSet { get; protected set; }

		[JsonIgnore]
		public override object ObjectValue {
			get => Value;
			set {
				if (value is T validValue) {
					SetValue = validValue;
				} else {
					throw new ArgumentException("ObjectValue must have the Type of the setting");
				}
			}
		}
		[JsonProperty(Order = 1)]
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

		protected internal Setting(SettingNames name, T defaultValue, string description) : base(name, description) => Default = defaultValue;
		public override event PropertyChangedEventHandler PropertyChanged;

		public override bool AddValueWithString(string value, int valueParameterIndex = 1, string key = null, int? keyParameterIndex = null) =>
			throw new NotImplementedException();

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public override bool SetValueWithString(string value, int parameterIndex = 1) {
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
	}
}
