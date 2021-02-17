#region
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Kudos.Bot;
using Kudos.Exceptions;
using Kudos.Models;
#endregion

namespace Kudos.Extensions {
	public static class StringExtensions {
		private static readonly Regex NotNormalChars = new Regex("[^a-zA-Z]");
		private static readonly Regex NotUniqueChars = new Regex("(.).*\\1");
		public static bool JustNormalChars(this string text) => !NotNormalChars.IsMatch(text);

		public static string LikeInput(this TimeSpan timeSpan) {
			string readable = string.Empty;
			if ((int)timeSpan.TotalDays > 0) {
				readable += $"{(int)timeSpan.TotalDays}d";
			}
			if (timeSpan.Hours > 0) {
				readable += $"{timeSpan.Hours}h";
			}
			if (timeSpan.Minutes > 0) {
				readable += $"{timeSpan.Minutes}m";
			}
			if (timeSpan.Seconds > 0) {
				readable += $"{timeSpan.Seconds}s";
			}
			if (readable == "") {
				readable += "0";
			}
			return readable;
		}

		public static string Readable(this TimeSpan timeSpan) {
			string readable = string.Empty;
			if ((int)timeSpan.TotalDays > 0) {
				readable += $"{(int)timeSpan.TotalDays} Days ";
			}
			if (timeSpan.Hours > 0) {
				readable += $"{timeSpan.Hours} Hours ";
			}
			if (timeSpan.Minutes > 0) {
				readable += $"{timeSpan.Minutes} Minutes ";
			}
			if (timeSpan.Seconds > 0) {
				readable += $"{timeSpan.Seconds} Seconds ";
			}
			if (readable == "") {
				readable += "now ";
			}
			return readable;
		}

		public static string[] SplitAtSpace(this string text, int maxParts, int maxPartLength) {
			List<string> parts = new List<string>();
			for (int i = 0; i < maxParts; i++) {
				if (text.Length < maxPartLength) {
					parts.Add(text);
					break;
				}
				int spaceIndex = text.LastIndexOf(' ', Math.Min(maxPartLength, text.Length - 1));
				if (spaceIndex == -1 || spaceIndex < text.Length - (maxParts - (i + 1)) * maxPartLength) {
					spaceIndex = maxPartLength - 1;
				}

				parts.Add(text.Remove(spaceIndex));
				text = text.Substring(spaceIndex + 1);
			}
			return parts.ToArray();
		}

		public static TValue ToValue<TValue>(this string value, int parameterIndex, Settings settings) {
			string[] arr = new string[parameterIndex + 1];
			arr[parameterIndex] = value;
			try {
				return ParameterType.InterpretParameter(arr, default, parameterIndex, false, new ParameterType.DefaultValue<TValue>(), default, default,
					false, settings);
			}
			catch (KudosArgumentTypeException exception) {
				throw new KudosArgumentTypeException(exception.UserMessage + " as text", exception.Message + " as text");
			}
		}

		public static bool UniqueChars(this string text) => !NotUniqueChars.IsMatch(text);
	}
}
