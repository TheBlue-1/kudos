#region
using System;
using System.Collections.Concurrent;
using Discord.WebSocket;
#endregion

namespace Kudos.Utils {
	public class ParameterType {
		public char Character { get; }

		public string Description { get; }
		public static ConcurrentDictionary<Type, ParameterType> ParameterTypes { get; } = new ConcurrentDictionary<Type, ParameterType>();
		public Type Type { get; }

		static ParameterType() {
			ParameterType boolParameter = new ParameterType('b', typeof (bool), "true or false");
			ParameterTypes.TryAdd(boolParameter.Type, boolParameter);
			ParameterType intParameter = new ParameterType('n', typeof (int), "a number");
			ParameterTypes.TryAdd(intParameter.Type, intParameter);
			ParameterType ulongParameter = new ParameterType('p', typeof (ulong), "a positive number");
			ParameterTypes.TryAdd(ulongParameter.Type, ulongParameter);
			ParameterType stringParameter = new ParameterType('t', typeof (string), "a text to the end of the message o surrounded with  like \"this\"");
			ParameterTypes.TryAdd(stringParameter.Type, stringParameter);
			ParameterType userParameter = new ParameterType('u', typeof (SocketUser), "a user mention or @username#number");
			ParameterTypes.TryAdd(userParameter.Type, userParameter);
		}

		private ParameterType(char character, Type type, string description) {
			Character = character;
			Type = type;
			Description = description;
		}

		public override string ToString() => $"`{Character}` {Description}";
	}
}
