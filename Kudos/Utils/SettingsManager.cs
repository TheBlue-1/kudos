#region

using Discord.WebSocket;
using Kudos.Models;
using System.Diagnostics.CodeAnalysis;

#endregion

namespace Kudos.Utils {

    public sealed class SettingsManager {
        private SettingsDictionary GuildSettings { get; } = new("guildSettings");

        public static SettingsManager Instance { get; } = new();
        private SettingsDictionary UserSettings { get; } = new("userSettings");

        static SettingsManager() {
        }

        private SettingsManager() {
        }

        public void DeleteGuildSettings(ulong guildId) {
            GuildSettings.Remove(guildId);
        }

        public void DeleteUserSettings(ulong userId) {
            UserSettings.Remove(userId);
        }

        public Settings SettingsFor(SocketMessage message, bool create = false) =>
            SettingsFor(message.Author.Id, (message.Channel as SocketGuildChannel)?.Guild.Id, create);

        public Settings SettingsFor(SocketUser author, ISocketMessageChannel channel, bool create = false) =>
            SettingsFor(author.Id, (channel as SocketGuildChannel)?.Guild.Id, create);

        [SuppressMessage("ReSharper", "PatternAlwaysOfType")]
        public Settings SettingsFor(ulong? userId = null, ulong? guildId = null, bool create = false) {
            ulong userIdLong = 0;
            ulong guildIdLong = 0;
            bool hasUser = false;
            bool hasGuild = false;
            if (userId is ulong userIdAsLong) {
                userIdLong = userIdAsLong;
                hasUser = true;
            }
            if (guildId is ulong guildIdAsLong) {
                guildIdLong = guildIdAsLong;
                hasGuild = true;
            }

            if (hasUser && UserSettings.ContainsKey(userIdLong)) {
                if (hasGuild && GuildSettings.ContainsKey(guildIdLong)) {
                    return GuildSettings[guildIdLong].Merge(UserSettings[userIdLong]);
                }
                return UserSettings[userIdLong];
            }
            if (hasGuild && GuildSettings.ContainsKey(guildIdLong)) {
                return GuildSettings[guildIdLong];
            }
            Settings settings = new();
            if (!create || hasUser == hasGuild) {
                return settings;
            }
            if (hasUser) {
                UserSettings[userIdLong] = settings;
            } else {
                GuildSettings[guildIdLong] = settings;
            }
            return settings;
        }
    }
}