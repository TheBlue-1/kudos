#region
using System;
#endregion

namespace Kudos.Exceptions {
	internal class KudosArgumentException : ArgumentException, IKudosException {
		public string UserMessage { get; }

		public KudosArgumentException(string userMessage, string message = null) : base(message ?? userMessage) => UserMessage = userMessage;
	}
}
