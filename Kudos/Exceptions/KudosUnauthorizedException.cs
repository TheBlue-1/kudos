#region
using System;
#endregion

namespace Kudos.Exceptions {
	internal class KudosUnauthorizedException : Exception, IKudosException {
		public TimeSpan? Lifetime { get; }
		public string UserMessage { get; }

		public KudosUnauthorizedException(string userMessage, string message = null, TimeSpan? lifetime = null) : base(message ?? userMessage) {
			UserMessage = userMessage;
			Lifetime = lifetime;
		}
	}
}
