#region
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Discord;
using Discord.WebSocket;
using Kudos.Exceptions;
using Kudos.Models;
using UserExtensions = Kudos.Extensions.UserExtensions;
#endregion

namespace Kudos.Bot {
	public class ParameterType {
		public delegate object ParameterAsType(string[] parameters, IEnumerable<object> indexLess, int index, bool optional, Optional<object> defaultValue,
			object min, object max, bool throwOutOfRange);

		public char Character { get; }

		public string Description { get; }

		public ParameterAsType ParameterInterpreter { get; }
		public static ConcurrentDictionary<Type, ParameterType> ParameterTypes { get; } = new ConcurrentDictionary<Type, ParameterType>();
		public Type Type { get; }

		static ParameterType() {
			ParameterType boolParameter = new ParameterType(typeof (bool), 'b', "true or false", ParameterAsBool);
			ParameterTypes.TryAdd(boolParameter.Type, boolParameter);
			ParameterType intParameter = new ParameterType(typeof (int), 'n', "a number", ParameterAsInt);
			ParameterTypes.TryAdd(intParameter.Type, intParameter);
			ParameterType ulongParameter = new ParameterType(typeof (ulong), 'p', "a positive number", ParameterAsULong);
			ParameterTypes.TryAdd(ulongParameter.Type, ulongParameter);
			ParameterType stringParameter = new ParameterType(typeof (string), 't', "a text to the end of the message o surrounded with  like \"this\"",
				ParametersAsString);
			ParameterTypes.TryAdd(stringParameter.Type, stringParameter);
			ParameterType userParameter = new ParameterType(typeof (SocketUser), 'u', "a user mention or @username#number", ParameterAsSocketUser);
			ParameterTypes.TryAdd(userParameter.Type, userParameter);
			ParameterType emojiParameter = new ParameterType(typeof (Emoji), 'e', "an emoji", ParameterAsEmoji);
			ParameterTypes.TryAdd(emojiParameter.Type, emojiParameter);
			ParameterType settingsParameter = new ParameterType(typeof (Settings));
			ParameterTypes.TryAdd(settingsParameter.Type, settingsParameter);
			ParameterType messageParameter = new ParameterType(typeof (SocketMessage));
			ParameterTypes.TryAdd(messageParameter.Type, messageParameter);
			ParameterType channelParameter = new ParameterType(typeof (ISocketMessageChannel));
			ParameterTypes.TryAdd(channelParameter.Type, channelParameter);
		}

		private ParameterType(Type type, char character = default, string description = null, ParameterAsType parameterInterpreter = null) {
			Character = character;
			Type = type;
			Description = description;
			ParameterInterpreter = parameterInterpreter ?? NotImplemented;

			static object NotImplemented(string[] parameters, IEnumerable<object> indexLess, int index, bool optional, Optional<object> defaultValue,
				object min, object max, bool throwOutOfRange) => throw new NotImplementedException();
		}

		[SuppressMessage("ReSharper", "InvertIf")]
		private static T CheckMinMax<T>(T value, int index, object min, object max, bool throwOutOfRange)
			where T : IComparable {
			if (min != null) {
				if (value.CompareTo(min) < 0) {
					if (throwOutOfRange) {
						throw new KudosArgumentOutOfRangeException($"parameter {index} must be bigger than {min}");
					}
					return (T)min;
				}
			}
			if (max != null) {
				if (value.CompareTo(max) > 0) {
					if (throwOutOfRange) {
						throw new KudosArgumentOutOfRangeException($"parameter {index} must be smaller than {max}");
					}
					return (T)max;
				}
			}

			return value;
		}

		public object IndexLess(IEnumerable<object> indexLess) {
			MethodInfo method = GetType().GetMethod(nameof (SetIndexLess))?.MakeGenericMethod(Type);
			return method?.Invoke(this, new object[] { indexLess });
		}

		private static object ParameterAsBool(string[] parameters, IEnumerable<object> indexLess, int index, bool optional, Optional<object> defaultValue,
			object min, object max, bool throwOutOfRange) {
			if (parameters.Length > index && bool.TryParse(parameters[index], out bool value)) {
				return value;
			}
			if (optional) {
				SetToDefaultValue(out value, defaultValue, indexLess);
			} else {
				throw new KudosArgumentException($"Parameter {index + 1} must be a true/false (bool)");
			}
			return value;
		}

		private static object ParameterAsEmoji(string[] parameters, IEnumerable<object> indexLess, int index, bool optional, Optional<object> defaultValue,
			object min, object max, bool throwOutOfRange) {
			if (parameters.Length > index) {
				string emojiString = parameters[index];
				return new Emoji(emojiString);
			}
			if (!optional) {
				throw new KudosArgumentException($"Parameter {index + 1} must be a emoji (described in help)");
			}

			SetToDefaultValue(out Emoji emoji, defaultValue, indexLess);
			return emoji;
		}

		private static object ParameterAsInt(string[] parameters, IEnumerable<object> indexLess, int index, bool optional, Optional<object> defaultValue,
			object min, object max, bool throwOutOfRange) {
			if (parameters.Length > index && int.TryParse(parameters[index], out int value)) {
				return CheckMinMax(value, index, min, max, throwOutOfRange);
			}
			if (optional) {
				SetToDefaultValue(out value, defaultValue, indexLess);
			} else {
				throw new KudosArgumentException($"Parameter {index + 1} must be a number (int)");
			}
			return value;
		}

		private static object ParameterAsSocketUser(string[] parameters, IEnumerable<object> indexLess, int index, bool optional, Optional<object> defaultValue,
			object min, object max, bool throwOutOfRange) {
			SocketUser user;
			if (parameters.Length > index) {
				user = UserExtensions.FromMention(parameters[index]);
				if (user != null) {
					return user;
				}

				string[] userData = parameters[index].Split("#");
				if (userData.Length == 2) {
					user = Program.Client.GetSocketUserByUsername(userData[0], userData[1]);
				}
				if (user != null) {
					return user;
				}
			}
			if (!optional) {
				throw new KudosArgumentException($"Parameter {index + 1} must be a user (described in help)");
			}

			SetToDefaultValue(out user, defaultValue, indexLess);
			return user;
		}

		private static object ParameterAsULong(string[] parameters, IEnumerable<object> indexLess, int index, bool optional, Optional<object> defaultValue,
			object min, object max, bool throwOutOfRange) {
			if (parameters.Length > index && ulong.TryParse(parameters[index], out ulong value)) {
				return CheckMinMax(value, index, min, max, throwOutOfRange);
			}
			if (optional) {
				SetToDefaultValue(out value, defaultValue, indexLess);
			} else {
				throw new KudosArgumentException($"Parameter {index + 1} must be a number (ulong)");
			}
			return value;
		}

		private static object ParametersAsString(string[] parameters, IEnumerable<object> indexLess, int index, bool optional, Optional<object> defaultValue,
			object min, object max, bool throwOutOfRange) {
			if (parameters.Length > index) {
				if (parameters[index].StartsWith('"') && parameters[index].EndsWith('"')) {
					return parameters[index].Substring(1, parameters[index].Length - 2);
				}

				return string.Join(" ", parameters.Skip(index));
			}
			if (!optional) {
				throw new KudosArgumentException($"Parameter {index + 1} must be a text");
			}
			SetToDefaultValue(out string value, defaultValue, indexLess);
			return value;
		}

		public static T SetIndexLess<T>(IEnumerable<object> indexLess) {
			foreach (object obj in indexLess) {
				if (obj is T param) {
					return param;
				}
			}
			throw new Exception("Parameter doesn't exist");
		}

		public static void SetToDefaultValue<T>(out T value, Optional<object> defaultValue, IEnumerable<object> indexLess) {
			if (defaultValue.IsSpecified) {
				if (defaultValue.Value.Equals(SpecialDefaults.IndexLess)) {
					value = SetIndexLess<T>(indexLess);
					return;
				}
				value = (T)defaultValue.Value;
				return;
			}
			value = default;
		}

		public override string ToString() => Description == null ? string.Empty : $"`{Character}` {Description}";

		public enum SpecialDefaults {
			IndexLess
		}
	}
}
