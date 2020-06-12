#region
using System.Collections.Generic;
#endregion

namespace Kudos.Exceptions {
	internal class KudosKeyNotFoundException : KeyNotFoundException, IKudosException {
		public string UserMessage { get; }

		public KudosKeyNotFoundException(string userMessage, string message = null) : base(message ?? userMessage) => UserMessage = userMessage;
	}
}
