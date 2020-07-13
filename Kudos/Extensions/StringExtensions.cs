#region
using System.Text.RegularExpressions;
using Discord;
using Kudos.Bot;
#endregion

namespace Kudos.Extensions {
	public static class StringExtensions {
		private static readonly Regex NotNormalChars = new Regex("[^a-z]");
		private static readonly Regex NotUniqueChars = new Regex("(.).*\\1");
		public static bool JustNormalChars(this string text) => !NotNormalChars.IsMatch(text);
		public static bool UniqueChars(this string text) => !NotUniqueChars.IsMatch(text);
		public static TValue ToValue<TValue>( this string value,int parameterIndex) {
			var arr = new string[parameterIndex + 1];
			arr[parameterIndex] = value;
			TValue newValue = (TValue)ParameterType.ParameterTypes[typeof(TValue)]
				.ParameterInterpreter.Invoke(arr, new object[0], parameterIndex, false, Optional<object>.Unspecified, null, null,
					false);
			return newValue;
		}
	}
}
