#region
using System.Collections.Immutable;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Kudos.Exceptions;
#endregion

namespace Kudos.Models.bases {
	public abstract class SettingBase : INotifyPropertyChanged {
		public abstract string HelpText { get; }
		public abstract string HtmlHelpText { get; }
		public abstract string StringValue { get; }
		[JsonIgnore]
		public string Description { get; }
		public SettingNames Name { get; }

		public abstract bool IsSet { get; protected set; }

		public abstract object ObjectValue { get; set; }

		protected SettingBase(SettingNames name, string description) {
			Name = name;
			Description = description;
		}

		public abstract bool AddValueWithString(string value, Settings settings, int valueParameterIndex = 1, string key = null, int? keyParameterIndex = null);

		public abstract SettingBase Merge(SettingBase guildSetting);

		public abstract bool SetValueWithString(string value, Settings settings, int parameterIndex = 1);

		public bool AddOrSetValue(string value, Settings settings, int valueParameterIndex = 1, string key = null, int? keyParameterIndex = null) =>
			this is IDictionarySetting
				? AddValueWithString(value, settings, valueParameterIndex, key, keyParameterIndex)
				: SetValueWithString(value, settings, valueParameterIndex);

		public static Setting<T> Create<T>(SettingNames name, T defaultValue, string description) => new Setting<T>(name, defaultValue, description);

		public static ListSetting<T> Create<T>(SettingNames name, ImmutableHashSet<T> defaultValue, string description) =>
			new ListSetting<T>(name, defaultValue, description);

		public static DictionarySetting<T1, T2> Create<T1, T2>(SettingNames name, ImmutableDictionary<T1, T2> defaultValue, string description) =>
			new DictionarySetting<T1, T2>(name, defaultValue, description);

		protected void SameTypeCheck(SettingBase setting) {
			if (setting.GetType() == GetType()) {
				return;
			}
			throw new KudosInternalException("setting must be of the same type");
		}

		public void Value<T>(out T value) {
			value = this is Setting<T> setting ? setting.Value : default;
		}

		public abstract event PropertyChangedEventHandler PropertyChanged;
	}
}
