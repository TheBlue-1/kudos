﻿#region

using Kudos.Bot;
using Kudos.Exceptions;
using Kudos.Extensions;
using Kudos.Models.bases;
using System.Collections.Immutable;

#endregion

namespace Kudos.Models {

    public class ListSetting<T> : Setting<ImmutableHashSet<T>>, IListSetting {

        public override string HelpText {
            get {
                string info = $"[{ParameterType.FromType(typeof(T)).Character}|value]";
                string text = $"`{Name.ToString().ToLower()} {info}` {Description}";
                return text;
            }
        }

        public override string HtmlHelpText {
            get {
                string info = $"[{ParameterType.FromType(typeof(T)).Character}|value]";
                string text = $"<tr><td><b>{Name.ToString().ToLower()} {info}</b></td><td>{Description}</td></tr>";
                return text;
            }
        }

        protected internal ListSetting(SettingNames name, ImmutableHashSet<T> defaultValue, string description) : base(name, defaultValue, description) {
        }

        public override SettingBase Merge(SettingBase guildSetting) {
            SameTypeCheck(guildSetting);
            if (!IsSet) {
                return guildSetting;
            }
            if (!guildSetting.IsSet) {
                return this;
            }
            ListSetting<T> setting = Create(Name, Default, Description);
            setting.SetValue = ((ListSetting<T>)guildSetting).Value.Union(Value);
            return setting;
        }

        public override bool SetValueWithString(string value, Settings settings, int parameterIndex = 1) {
            if (Value.Count >= 20) {
                throw new KudosInvalidOperationException(
                    "You have reached the current limit of 20 values per setting. If you have a valid reason to use more than that please get in touch with our support team on our Support server.");
            }
            if (value == null) {
                throw new KudosArgumentException("value must be set in a list setting");
            }

            //index 1 because settings get values on index 1
            T newValue = value.ToValue<T>(parameterIndex, settings);
            if (newValue == null) {
                throw new KudosInternalException("value shouldn't be null");
            }
            if (Value.Contains(newValue)) {
                SetValue = Value.Remove(newValue);
                return false;
            }
            SetValue = Value.Add(newValue);
            return true;
        }
    }
}