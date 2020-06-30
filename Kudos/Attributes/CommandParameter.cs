#region
using System;
using System.Collections.Generic;
using System.Reflection;
using Discord;
using Discord.WebSocket;
using Kudos.Bot;
using Kudos.Extensions;
#endregion

namespace Kudos.Attributes {
	[AttributeUsage(AttributeTargets.Parameter)]
	public class CommandParameter : Attribute {
		public Optional<object> DefaultValue { get; }
		public int Index { get; }
		public object Max { get; }
		public object Min { get; }
		public bool Optional { get; }

		public bool ThrowOutOfRange { get; }

		private CommandParameter(int index = -1, object min = null, object max = null, bool optional = true, bool throwOutOfRange = false,
			object defaultValue = null) {
			Index = index;
			Min = min;
			Max = max;
			Optional = optional;

			DefaultValue = defaultValue == null ? new Optional<object>() : new Optional<object>(defaultValue);
			ThrowOutOfRange = throwOutOfRange;
		}

		public CommandParameter() : this(optional: false) { }
		public CommandParameter(int index) : this(index, optional: false) { }

		public CommandParameter(int index, object defaultValue, object min = null, object max = null) : this(index, defaultValue: defaultValue, min: min,
			max: max, optional: true) { }

		public object FormParameter(ParameterInfo info, string[] parameters, SocketMessage message) {
			IEnumerable<object> indexLess = new object[] { message.Author, message.Channel, message, message.Settings() };

			ParameterType parameterType = ParameterType.ParameterTypes[info.ParameterType];

			if (Index < 0) {
				return parameterType.IndexLess(indexLess);
			}

			return parameterType.ParameterInterpreter(parameters, indexLess, Index, Optional, DefaultValue, Min, Max,
				ThrowOutOfRange);
		}
	}
}
