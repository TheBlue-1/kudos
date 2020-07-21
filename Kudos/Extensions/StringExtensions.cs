#region
using System.Text.RegularExpressions;
using Discord;
using Kudos.Bot;
using Kudos.Exceptions;
#endregion

namespace Kudos.Extensions {
	public static class StringExtensions {
		private static readonly Regex NotNormalChars = new Regex("[^a-zA-Z]");
		private static readonly Regex NotUniqueChars = new Regex("(.).*\\1");
		public static bool JustNormalChars(this string text) => !NotNormalChars.IsMatch(text);

		public static TValue ToValue<TValue>(this string value, int parameterIndex) {
			string[] arr = new string[parameterIndex + 1];
			arr[parameterIndex] = value;
			try {
				TValue newValue = (TValue)ParameterType.FromType(typeof (TValue))
					.ParameterInterpreter.Invoke(arr, new object[0], parameterIndex, false, Optional<object>.Unspecified, null, null,
						false);
				return newValue;
			}
			catch (KudosArgumentTypeException exception) {
				throw new KudosArgumentTypeException(exception.UserMessage + " as text", exception.Message + " as text");
			}
		}

		public static bool UniqueChars(this string text) => !NotUniqueChars.IsMatch(text);
	}
}
