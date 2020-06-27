#region
using System.Threading.Tasks;
using Discord.WebSocket;
using Kudos.Attributes;
using Kudos.Exceptions;
using Kudos.Utils;

// ReSharper disable UnusedMember.Global
#endregion

namespace Kudos.Bot.Modules {
	[CommandModule("Settings")]
	public sealed class SettingsCommands {
		public static SettingsCommands Instance { get; } = new SettingsCommands();

		static SettingsCommands() { }

		private SettingsCommands() { }

		[Command("prefix")]
		public async Task SetPrefix([CommandParameter] ISocketMessageChannel channel, [CommandParameter(0)] string prefix,
			[CommandParameter(1, false)] bool forServer, [CommandParameter] SocketUser author) {
			if (forServer) {
				if (channel is SocketGuildChannel guildChannel) {
					SettingsManager.Instance.SettingsFor(guildId: guildChannel.Guild.Id, create: true).Prefix.SetValue = prefix;
				} else {
					throw new KudosArgumentException("Can not set server settings in pms");
				}
			} else {
				SettingsManager.Instance.SettingsFor(author.Id, create: true).Prefix.SetValue = prefix;
			}

			await Messaging.Instance.SendMessage(channel, $"prefix set to `{prefix}`");
		}
	}
}
