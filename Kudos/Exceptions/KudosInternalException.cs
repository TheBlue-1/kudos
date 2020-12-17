#region
using System;
#endregion

namespace Kudos.Exceptions {
	public class KudosInternalException : Exception, IKudosException {
		public TimeSpan? Lifetime { get; } = null;
		public string UserMessage { get; }

		public KudosInternalException(string message) : base(message) => UserMessage = "internal error";
	}
}
