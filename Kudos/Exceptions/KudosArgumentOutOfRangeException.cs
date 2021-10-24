#region

using System;

#endregion

namespace Kudos.Exceptions {

    public class KudosArgumentOutOfRangeException : ArgumentOutOfRangeException, IKudosException {
        public TimeSpan? Lifetime { get; }
        public string UserMessage { get; }

        public KudosArgumentOutOfRangeException(string userMessage, string message = null, TimeSpan? lifetime = null) : base(message ?? userMessage) {
            UserMessage = userMessage;
            Lifetime = lifetime;
        }
    }
}