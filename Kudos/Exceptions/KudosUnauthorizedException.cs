#region
using System;
#endregion

namespace Kudos.Exceptions {
	internal class KudosUnauthorizedException : Exception, IKudosException {
		public string UserMessage { get; }
		public KudosUnauthorizedException(string userMessage, string message = null) : base(message ?? userMessage) => UserMessage = userMessage;
	}
}
