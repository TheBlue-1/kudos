using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Kudos.Exceptions;
using Kudos.Extensions;

namespace Kudos.Models
{
	public class DictionarySetting<T1,T2>:Setting<ImmutableDictionary<T1,T2>> 
    {
		public DictionarySetting(ImmutableDictionary<T1, T2> defaultValue) : base(defaultValue) { }

		public override bool AddValueWithString(string value, int valueParameterIndex, string key, int? keyParameterIndex) {
			if (key==null||!(keyParameterIndex is int notNullKeyParameterIndex)) {
				throw new KudosInternalException("dictionarySetting needs key and keyParameterIndex");
			}
			T1 keyValue = key.ToValue<T1>(notNullKeyParameterIndex);
			if (keyValue == null)
			{
				throw new KudosInternalException("value shouldn't be null");
			}
			if (value == null) {
				SetValue = Value.Remove(keyValue);
				return false;
			}
			T2 valueValue = value.ToValue<T2>(valueParameterIndex);

			if (valueValue == null)
			{
				throw new KudosInternalException("value shouldn't be null");
			}
			SetValue = Value.SetItem(keyValue, valueValue);
			return true;
		}
	}
}
