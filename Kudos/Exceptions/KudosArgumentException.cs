#region

using System;

#endregion

namespace Kudos.Exceptions {

    internal class KudosArgumentException : ArgumentException, IKudosException {
        public TimeSpan? Lifetime { get; }
        public string UserMessage { get; }

        public KudosArgumentException(string userMessage, string message = null, TimeSpan? lifetime = null) : base(message ?? userMessage) {
            UserMessage = userMessage;
            Lifetime = lifetime;
        }
    }
}