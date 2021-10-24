#region

using System;

#endregion

namespace Kudos.Exceptions {

    internal interface IKudosException {
        TimeSpan? Lifetime { get; }
        string Message { get; }
        string UserMessage { get; }
    }
}