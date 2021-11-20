using Discord;
using Kudos.DatabaseModels;
using System;
using System.Collections.Generic;

namespace Kudos.Models {

    public class CallData {
        public IList<IUserMessage> CurrentInvites { get; set; }
        public DateTime Timeout { get; set; }

        public DateTime Start { get; } = DateTime.Now;

        public IUser StartedBy { get; }
        public GroupData Group { get; }
        public Discord.WebSocket.SocketVoiceChannel Channel { get; }

        public CallData(IUser startedBy, GroupData group, Discord.WebSocket.SocketVoiceChannel channel) {
            StartedBy = startedBy;
            Group = group;
            Channel = channel;
        }
    }
}