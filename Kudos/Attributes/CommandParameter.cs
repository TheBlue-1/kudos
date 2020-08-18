#region
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Discord;
using Discord.WebSocket;
using Kudos.Bot;
using Kudos.Extensions;
#endregion

namespace Kudos.Attributes {
	[AttributeUsage(AttributeTargets.Parameter)]
	public class CommandParameter : Attribute {
		public ParameterType.DefaultValue<object> DefaultValue { get; }
		public int Index { get; }
		public Optional<object> Max { get; }
		public Optional<object> Min { get; }
		public bool Optional { get; }

		public bool ThrowOutOfRange { get; }

		private CommandParameter(int index = -1, object min = null, object max = null, bool optional = true, bool throwOutOfRange = false,
			object defaultValue = null) {
			Index = index;
			Min = min == null ? new Optional<object>() : new Optional<object>(min);

			Max = max == null ? new Optional<object>() : new Optional<object>(max);
			Optional = optional;

			DefaultValue = ParameterType.DefaultValue<object>.Create(defaultValue);
			ThrowOutOfRange = throwOutOfRange;
		}

		public CommandParameter() : this(optional: false) { }
		public CommandParameter(int index) : this(index, optional: false) { }

		public CommandParameter(int index, object defaultValue, object min = null, object max = null) : this(index, defaultValue: defaultValue, min: min,
			max: max, optional: true) { }

		public object FormParameter(ParameterInfo info, string[] parameters, SocketMessage message) {
			IEnumerable<object> indexLess = new object[] { message.Author, message.Channel, message, message.Settings() };

			if (Index >= 0) {
				return ParameterType.InterpretParameter(info.ParameterType, parameters, indexLess, Index, Optional, DefaultValue, Min,
					Max, ThrowOutOfRange);
			}
			object value = indexLess.FirstOrDefault(obj => obj.GetType() == info.ParameterType)
				?? indexLess.FirstOrDefault(obj => info.ParameterType.IsInstanceOfType(obj));
			return value;
		}
	}
}
