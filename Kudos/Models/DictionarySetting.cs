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
		public override string HtmlHelpText {
			get {
				string info = $"[{ParameterType.FromType(typeof (T2)).Character}|value] [{ParameterType.FromType(typeof (T1)).Character}|key]";
				string text = $"<tr><td><b>{Name.ToString().ToLower()} {info}</b></td><td>{Description}</td></tr>";
				return text;
			}
		}

		protected internal DictionarySetting(SettingNames name, ImmutableDictionary<T1, T2> defaultValue, string description) : base(name, defaultValue,
			description) { }

		public override bool AddValueWithString(string value, Settings settings, int valueParameterIndex = 1, string key = null,
			int? keyParameterIndex = null) {
			if (Value.Count >= 20) {
				throw new KudosInvalidOperationException(
					"You have reached the current limit of 20 values per setting. If you have a valid reason to use more than that please get in touch with our support team on our Support server.");
			}
			if (key == null || !(keyParameterIndex is int notNullKeyParameterIndex)) {
				throw new KudosArgumentException($"{Name} is a dictionary setting so it needs a 'key'");
			}
			T1 keyValue = key.ToValue<T1>(notNullKeyParameterIndex, settings);
			if (keyValue == null) {
				throw new KudosInternalException("value shouldn't be null");
			}
			if (value == null) {
				SetValue = Value.Remove(keyValue);
				return false;
			}
			T2 valueValue = value.ToValue<T2>(valueParameterIndex, settings);

			if (valueValue == null) {
				throw new KudosInternalException("value shouldn't be null");
			}
			SetValue = Value.SetItem(keyValue, valueValue);
			return true;
		}

		public override SettingBase Merge(SettingBase guildSetting) {
			SameTypeCheck(guildSetting);
			if (!IsSet) {
				return guildSetting;
			}
			if (!guildSetting.IsSet) {
				return this;
			}
			DictionarySetting<T1, T2> setting = Create(Name, Default, Description);
			setting.SetValue = ((DictionarySetting<T1, T2>)guildSetting).Value.SetItems(Value);
			return setting;
		}
	}
}
