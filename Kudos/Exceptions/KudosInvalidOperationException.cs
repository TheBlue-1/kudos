#region
using System.Collections.Generic;
#endregion

namespace Kudos.Exceptions {
	internal class KudosInvalidOperationException : KeyNotFoundException, IKudosException {
		public string UserMessage { get; }

		public KudosInvalidOperationException(string userMessage, string message = null) : base(message ?? userMessage) => UserMessage = userMessage;
	}
}
