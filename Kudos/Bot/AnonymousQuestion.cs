using System;
using System.Collections.Generic;
using System.Text;

namespace Kudos.Bot
{
    public class AnonymousQuestion
    {
        /// <summary>
        /// TODO user sends bot a question private, bot sends it to receiver, bot sends answer back to questionnaire
		/// </summary>

		public static AnonymousQuestion Instance { get; } = new AnonymousQuestion();

		static AnonymousQuestion() { }

		private AnonymousQuestion() { }

    }
}
