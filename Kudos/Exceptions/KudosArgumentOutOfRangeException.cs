#region
using System;
#endregion

namespace Kudos.Exceptions {
	public class KudosArgumentOutOfRangeException : ArgumentOutOfRangeException, IKudosException {
		public string UserMessage { get; }

		public KudosArgumentOutOfRangeException(string userMessage, string message = null) : base(message ?? userMessage) => UserMessage = userMessage;
	}
}
