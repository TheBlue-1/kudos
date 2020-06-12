#region
using System;
#endregion

namespace Kudos.Exceptions {
	internal class KudosUnauthorizedAccessException : UnauthorizedAccessException, IKudosException {
		public string UserMessage { get; }
		public KudosUnauthorizedAccessException(string userMessage, string message = null) : base(message ?? userMessage) => UserMessage = userMessage;
	}
}
