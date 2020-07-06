#region
using System.Text.RegularExpressions;
#endregion

namespace Kudos.Extensions {
	public static class StringExtensions {
		private static readonly Regex NotNormalChars = new Regex("[^a-z]");
		private static readonly Regex NotUniqueChars = new Regex("(.).*\\1");
		public static bool JustNormalChars(this string text) => !NotNormalChars.IsMatch(text);
		public static bool UniqueChars(this string text) => !NotUniqueChars.IsMatch(text);
	}
}
