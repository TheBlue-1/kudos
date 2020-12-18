#region
using Kudos.Extensions;
#endregion

namespace Kudos.Models {
	public class Word {
		public string Value { get; }

		private Word(string word) => Value = word;

		public static Word Create(string word) => word.JustNormalChars() ? new Word(word) : null;
		public override string ToString() => Value;

		public static implicit operator string(Word word) => word.Value;
		public static implicit operator Word(string word) => Create(word);
	}
}
