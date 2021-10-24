#region

using Discord.WebSocket;
using Kudos.Models;
using Kudos.Utils;

#endregion

namespace Kudos.Extensions {

    public static class MessageExtensions {

        public static Settings Settings(this SocketMessage message) =>
            SettingsManager.Instance.SettingsFor(message.Author.Id, (message.Channel as SocketGuildChannel)?.Guild?.Id);
    }
}