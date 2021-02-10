#region
using Kudos.Exceptions;
using Kudos.Extensions;
#endregion

namespace Kudos.Models {
	public class Word {
		public string Value { get; }

		private Word(string word) {
			if (!word.JustNormalChars()) {
				throw new KudosArgumentException("A word can only contain normal characters (a-z)");
			}
			Value = word;
		}

		private static Word Create(string word) => new Word(word);
		public override string ToString() => Value;

		public static implicit operator string(Word word) => word.Value;
		public static implicit operator Word(string word) => Create(word);
	}
}
