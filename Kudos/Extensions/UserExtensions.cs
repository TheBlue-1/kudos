#region
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
#endregion

namespace Kudos.Extensions {
	public static class UserExtensions {
		public static ISocketAudioChannel AudioChannel(this SocketUser user) {
			IEnumerable<SocketGuildUser> guildUsers = user.MutualGuilds.Select(guild => guild.GetUser(user.Id));
			return guildUsers.FirstOrDefault(guildUser => guildUser.VoiceChannel != null)?.VoiceChannel;
		}

		public static async Task<IDMChannel> DmChannel(this SocketUser user) {
			RestUser restUser = await user.RestUser();
			IDMChannel channel = await restUser.GetOrCreateDMChannelAsync();
			return channel;
		}

		public static SocketUser FromMention(string mention) {
			Regex regex = new Regex("^<@!{0,1}(\\d+)>$");
			Match match = regex.Match(mention);
			if (!match.Success) {
				return null;
			}

			ulong id = ulong.Parse(match.Groups.Values.ToArray()[1].Value);
			return Program.Client.GetSocketUserById(id);
		}

		public static async Task<RestUser> RestUser(this SocketUser user) => await Program.Client.GetRestUserById(user.Id);
	}
}
