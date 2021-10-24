#region

using System;
using System.Collections.Generic;

#endregion

namespace Kudos.Exceptions {

    internal class KudosKeyNotFoundException : KeyNotFoundException, IKudosException {
        public TimeSpan? Lifetime { get; }
        public string UserMessage { get; }

        public KudosKeyNotFoundException(string userMessage, string message = null, TimeSpan? lifetime = null) : base(message ?? userMessage) {
            UserMessage = userMessage;
            Lifetime = lifetime;
        }
    }
}