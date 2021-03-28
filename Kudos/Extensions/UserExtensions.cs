#region
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;
#endregion

namespace Kudos.Extensions {
	public static class UserExtensions {
		private static readonly ulong[] BotAdmins = { 406118105711116288, 393152252862529556 };

		// ReSharper disable once UnusedMember.Global
		public static ISocketAudioChannel AudioChannel(this SocketUser user) {
			IEnumerable<SocketGuildUser> guildUsers = user.MutualGuilds.Select(guild => guild.GetUser(user.Id));
			return guildUsers.FirstOrDefault(guildUser => guildUser.VoiceChannel != null)?.VoiceChannel;
		}

		public static IMessageChannel ChannelFromMention(this string mention) {
			Regex regex = new("^<#(\\d+)>$");
			Match match = regex.Match(mention);
			if (!match.Success) {
				return null;
			}

			ulong id = ulong.Parse(match.Groups.Values.ToArray()[1].Value);
			return Program.Client.GetMessageChannelById(id);
		}

		public static SocketUser FromMention(this string mention) {
			Regex regex = new("^<@!{0,1}(\\d+)>$");
			Match match = regex.Match(mention);
			if (!match.Success) {
				return null;
			}

			ulong id = ulong.Parse(match.Groups.Values.ToArray()[1].Value);
			return Program.Client.GetSocketUserById(id);
		}

		// ReSharper disable once UnusedMember.Global
		public static bool HasRole(this IGuildUser user, params IRole[] roles) => user.HasRoleId(roles.Select(role => role.Id).ToArray());

		public static bool HasRoleId(this IGuildUser user, params ulong[] roleIds) =>
			(from roleId in roleIds from userRoleId in user.RoleIds where roleId == userRoleId select roleId).Any();

		public static bool IsBotAdmin(this IUser user) => BotAdmins.Contains(user.Id);

		public static bool IsGuildAdmin(this SocketGuildUser user) => user.GuildPermissions.Administrator;

		public static SocketRole RoleFromMention(this string mention) {
			Regex regex = new("^<@&{0,1}(\\d+)>$");
			Match match = regex.Match(mention);
			if (!match.Success) {
				return null;
			}

			ulong id = ulong.Parse(match.Groups.Values.ToArray()[1].Value);
			return Program.Client.GetRoleById(id);
		}
	}
}
