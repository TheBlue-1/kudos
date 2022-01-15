#region

using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

#endregion

namespace Kudos.Extensions {

    public static class UserExtensions {
        private static readonly ulong[] BotAdmins = { 406118105711116288, 393152252862529556 };

        // ReSharper disable once UnusedMember.Global
        public static ISocketAudioChannel AudioChannel(this SocketUser user) {
            IEnumerable<SocketGuildUser> guildUsers = user.MutualGuilds.Select(guild => guild.GetUser(user.Id));
            return guildUsers.FirstOrDefault(guildUser => guildUser.VoiceChannel != null)?.VoiceChannel;
        }

        public static IMessageChannel ChannelFromMentionOrId(this string mention) {
            if (!ulong.TryParse(mention, out ulong id)) {
                Regex regex = new("^<#(\\d+)>$");
                Match match = regex.Match(mention);
                if (!match.Success) {
                    return null;
                }
                id = ulong.Parse(match.Groups.Values.ToArray()[1].Value);
            }

            return Program.Client.GetMessageChannelById(id);
        }

        public static SocketUser UserFromMentionOrId(this string mention) {
            if (!ulong.TryParse(mention, out ulong id)) {
                Regex regex = new("^<@!{0,1}(\\d+)>$");
                Match match = regex.Match(mention);
                if (!match.Success) {
                    return null;
                }

                id = ulong.Parse(match.Groups.Values.ToArray()[1].Value);
            }
            return Program.Client.GetSocketUserById(id);
        }

        // ReSharper disable once UnusedMember.Global
        public static bool HasRole(this IGuildUser user, params IRole[] roles) => user.HasRoleId(roles.Select(role => role.Id).ToArray());

        public static bool HasRoleId(this IGuildUser user, params ulong[] roleIds) =>
            (from roleId in roleIds from userRoleId in user.RoleIds where roleId == userRoleId select roleId).Any();

        public static bool IsBotAdmin(this IUser user) => BotAdmins.Contains(user.Id);

        public static bool IsGuildAdmin(this SocketGuildUser user) => user.GuildPermissions.Administrator;

        public static SocketRole RoleFromMentionOrId(this string mention) {
            if (!ulong.TryParse(mention, out ulong id)) {
                Regex regex = new("^<@&{0,1}(\\d+)>$");
                Match match = regex.Match(mention);
                if (!match.Success) {
                    return null;
                }

                id = ulong.Parse(match.Groups.Values.ToArray()[1].Value);
            }
            return Program.Client.GetRoleById(id);
        }
    }
}