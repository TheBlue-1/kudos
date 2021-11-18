using Discord;
using System;
using System.Collections.Generic;

namespace Kudos.Models {

    public class CallData {
        public IList<IUserMessage> CurrentInvites { get; set; }
        public DateTime Timeout { get; set; }

        public DateTime Start { get; } = DateTime.Now;

        public IUser StartedBy { get; }

        public CallData(IUser startedBy) {
            StartedBy = startedBy;
        }
    }
}