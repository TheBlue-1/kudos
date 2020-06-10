#region
using Discord.WebSocket;
#endregion

namespace Kudos.Extensions {
	public static class SocketUserExtension {
		public static string Mention(this SocketUser user) => $"<@ {user.Id}>";
	}
}
