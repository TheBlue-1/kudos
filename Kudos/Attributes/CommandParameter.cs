﻿#region
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Discord;
using Discord.WebSocket;
using Kudos.Exceptions;
using Kudos.Extensions;
#endregion

namespace Kudos.Attributes {
	[AttributeUsage(AttributeTargets.Parameter)]
	public class CommandParameter : Attribute {
		public Optional<object> DefaultValue { get; }
		public int Index { get; }
		private IEnumerable<object> IndexLess => new object[] { Message.Author, Message.Channel, Message, Message.Settings() };
		public object Max { get; }
		public object Min { get; }
		public bool Optional { get; }

		public bool ThrowOutOfRange { get; }
		private SocketMessage Message { get; set; }

		private string[] Parameters { get; set; }

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

		[SuppressMessage("ReSharper", "InvertIf")]
		private T CheckMinMax<T>(T value)
			where T : IComparable {
			if (Min != null) {
				if (value.CompareTo(Min) < 0) {
					if (ThrowOutOfRange) {
						throw new KudosArgumentOutOfRangeException($"parameter {Index} must be bigger than {Min}");
					}
					return (T)Min;
				}
			}
			if (Max != null) {
				if (value.CompareTo(Max) > 0) {
					if (ThrowOutOfRange) {
						throw new KudosArgumentOutOfRangeException($"parameter {Index} must be smaller than {Max}");
					}
					return (T)Max;
				}
			}

			return value;
		}

		public object FormParameter(ParameterInfo info, string[] parameters, SocketMessage message) {
			Message = message;
			Parameters = parameters;
			Type type = info.ParameterType;
			if (Index < 0) {
				MethodInfo method = GetType().GetMethod(nameof (SetIndexLess))?.MakeGenericMethod(type);
				return method?.Invoke(this, null);
			}

			if (type == typeof (int)) {
				return ParameterAsInt();
			}
			if (type == typeof (bool)) {
				return ParameterAsBool();
			}
			if (type == typeof (ulong)) {
				return ParameterAsULong();
			}
			if (type == typeof (SocketUser)) {
				return ParameterAsSocketUser();
			}
			return type == typeof (string) ? ParametersAsString() : null;
		}

		private bool ParameterAsBool() {
			if (Parameters.Length > Index && bool.TryParse(Parameters[Index], out bool value)) {
				return value;
			}
			if (Optional) {
				SetToDefaultValue(out value);
			} else {
				throw new KudosArgumentException($"Parameter {Index + 1} must be a true/false (bool)");
			}
			return value;
		}

		private int ParameterAsInt() {
			if (Parameters.Length > Index && int.TryParse(Parameters[Index], out int value)) {
				return CheckMinMax(value);
			}
			if (Optional) {
				SetToDefaultValue(out value);
			} else {
				throw new KudosArgumentException($"Parameter {Index + 1} must be a number (int)");
			}
			return value;
		}

		private SocketUser ParameterAsSocketUser() {
			SocketUser user = Message.MentionedUsers.FirstOrDefault();
			if (user != null) {
				return user;
			}
			if (Parameters.Length > Index) {
				string[] userData = Parameters[Index].Split("#");
				if (userData.Length == 2) {
					user = Program.Client.GetSocketUserByUsername(userData[0], userData[1]);
				}
				if (user != null) {
					return user;
				}
			}
			if (!Optional) {
				throw new KudosArgumentException($"Parameter {Index + 1} must be a user (described in help)");
			}

			SetToDefaultValue(out user);
			return user;
		}

		private ulong ParameterAsULong() {
			if (Parameters.Length > Index && ulong.TryParse(Parameters[Index], out ulong value)) {
				return CheckMinMax(value);
			}
			if (Optional) {
				SetToDefaultValue(out value);
			} else {
				throw new KudosArgumentException($"Parameter {Index + 1} must be a number (ulong)");
			}
			return value;
		}

		private string ParametersAsString() {
			if (Parameters.Length > Index) {
				if (Parameters[Index].StartsWith('"') && Parameters[Index].EndsWith('"')) {
					return Parameters[Index].Substring(1, Parameters[Index].Length - 2);
				}

				return string.Join(" ", Parameters.Skip(Index - 1));
			}
			if (!Optional) {
				throw new KudosArgumentException($"Parameter {Index + 1} must be a text");
			}
			SetToDefaultValue(out string value);
			return value;
		}

		public T SetIndexLess<T>() {
			foreach (object obj in IndexLess) {
				if (obj is T param) {
					return param;
				}
			}
			throw new Exception("Parameter doesn't exist");
		}

		public void SetToDefaultValue<T>(out T value) {
			if (DefaultValue.IsSpecified) {
				if (DefaultValue.Value.Equals(SpecialDefaults.IndexLess)) {
					value = SetIndexLess<T>();
					return;
				}
				value = (T)DefaultValue.Value;
				return;
			}
			value = default;
		}

		public enum SpecialDefaults {
			IndexLess
		}
	}
}
