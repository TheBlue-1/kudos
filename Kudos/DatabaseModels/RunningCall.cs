using Discord;
using Discord.WebSocket;
using Kudos.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Kudos.DatabaseModels {

    public class RunningCall {

        [NotMapped]
        public IList<IUserMessage> CurrentInvites { get; set; }

        public IEnumerable<Tuple<ulong, ulong>> CurrentInvitesIds => CurrentInvites.Select(i => new Tuple<ulong, ulong>(i.Channel.Id, i.Id));
        public DateTime Timeout { get; set; }

        public DateTime Start { get; private set; } = DateTime.Now;

        [NotMapped]
        public IUser StartedBy { get; }

        public ulong StartedById { get => StartedBy.Id; private set { _ = value; } }

        [NotMapped]
        public GroupData Group { get; }

        [NotMapped]
        public SocketVoiceChannel Channel { get; }

        [Key]
        public ulong ChannelId { get => Channel.Id; private set { _ = value; } }

        public RunningCall(IUser startedBy, GroupData group, SocketVoiceChannel channel) {
            StartedBy = startedBy;
            Group = group;
            Channel = channel;
        }

        public RunningCall(DateTime timeout, DateTime start, ulong startedById, ulong channelId, IEnumerable<Tuple<ulong, ulong>> currentInvitesIds) {
            Timeout = timeout;
            Start = start;
            StartedBy = Program.Client.GetSocketUserById(startedById);
            if (StartedBy == null) {
                LogService.Instance.Log($"A server group call starter could not be found after restart", LogService.LogType.Main, Google.Cloud.Logging.Type.LogSeverity.Info);
            }
            Channel = (SocketVoiceChannel)Program.Client.GetVoiceChannelById(channelId);
            if (Channel == null) {
                LogService.Instance.Log($"A server group call channel could not be found after restart", LogService.LogType.Main, Google.Cloud.Logging.Type.LogSeverity.Info);
            }
            using (KudosDataContext db = new()) {
                Group = db.GroupData.Find(channelId);
            }
            if (Group == null) {
                LogService.Instance.Log($"A server group call group could not be found after restart", LogService.LogType.Main, Google.Cloud.Logging.Type.LogSeverity.Info);
            }

            CurrentInvites = currentInvitesIds.ToAsyncEnumerable().SelectAwait(async i => {
                var channel = Program.Client.GetMessageChannelById(i.Item1);
                if (channel == null) return null;
                var message = await channel.GetMessageAsync(i.Item2);
                return message as IUserMessage;
            }).ToListAsync().GetAwaiter().GetResult();
            CurrentInvites = CurrentInvites.Where(i => i != null).ToList();
        }
    }
}