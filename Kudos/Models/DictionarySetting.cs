#region
using System.Collections.Immutable;
using Kudos.Bot;
using Kudos.Exceptions;
using Kudos.Extensions;
using Kudos.Models.bases;
#endregion

namespace Kudos.Models {
	public class DictionarySetting<T1, T2> : Setting<ImmutableDictionary<T1, T2>>, IDictionarySetting {
		public override string HelpText {
			get {
				string info = $"[{ParameterType.FromType(typeof (T2)).Character}|value] [{ParameterType.FromType(typeof (T1)).Character}|key]";
				string text = $"`{Name.ToString().ToLower()} {info}` {Description}";
				return text;
			}
		}

		protected internal DictionarySetting(SettingNames name, ImmutableDictionary<T1, T2> defaultValue, string description) : base(name, defaultValue,
			description) { }

		public override bool AddValueWithString(string value, int valueParameterIndex = 1, string key = null, int? keyParameterIndex = null) {
			if (key == null || !(keyParameterIndex is int notNullKeyParameterIndex)) {
				throw new KudosInternalException("dictionarySetting needs key and keyParameterIndex");
			}
			T1 keyValue = key.ToValue<T1>(notNullKeyParameterIndex);
			if (keyValue == null) {
				throw new KudosInternalException("value shouldn't be null");
			}
			if (value == null) {
				SetValue = Value.Remove(keyValue);
				return false;
			}
			T2 valueValue = value.ToValue<T2>(valueParameterIndex);

			if (valueValue == null) {
				throw new KudosInternalException("value shouldn't be null");
			}
			SetValue = Value.SetItem(keyValue, valueValue);
			return true;
		}

		public override SettingBase Merge(SettingBase serverSetting) {
			SameTypeCheck(serverSetting);
			if (!IsSet) {
				return serverSetting;
			}
			if (!serverSetting.IsSet) {
				return this;
			}
			DictionarySetting<T1, T2> setting = Create(Name, Default, Description);
			setting.SetValue = ((DictionarySetting<T1, T2>)serverSetting).Value.SetItems(Value);
			return setting;
		}
	}
}
