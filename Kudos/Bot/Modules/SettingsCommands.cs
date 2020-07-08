#region
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Kudos.Attributes;
using Kudos.Exceptions;
using Kudos.Models;
using Kudos.Utils;

// ReSharper disable UnusedMember.Global
#endregion

namespace Kudos.Bot.Modules {
	[CommandModule("EditableSettings")]
	public sealed class SettingsCommands {
		public static SettingsCommands Instance { get; } = new SettingsCommands();

		static SettingsCommands() { }

		private SettingsCommands() { }

		private static Settings EditableSettings(bool forServer, ISocketMessageChannel channel, SocketUser author) {
			if (!forServer) {
				return SettingsManager.Instance.SettingsFor(author.Id, create: true);
			}
			if (channel is SocketGuildChannel guildChannel) {
				return SettingsManager.Instance.SettingsFor(guildId: guildChannel.Guild.Id, create: true);
			}

			throw new KudosArgumentException("Can not set server settings in pms");
		}

		// ReSharper disable once StringLiteralTypo
		[Command("autoreact", "set automatic reactions to messages")]
		public async Task SetAutoReact([CommandParameter] ISocketMessageChannel channel, [CommandParameter(0)] string message,
			[CommandParameter(1, null)] IEmote emoji, [CommandParameter(2, false)] bool forServer, [CommandParameter] SocketUser author) {
			Settings settings = EditableSettings(forServer, channel, author);
			settings.AutoReact.SetValue = settings.AutoReact.Value.SetItem(message, emoji.ToString());
			await Messaging.Instance.SendMessage(channel, emoji.ToString() == null ? $"unset for `{message}`" : $"set `{emoji}` for `{message}`");
		}

		[Command("prefix", "changes the prefix")]
		public async Task SetPrefix([CommandParameter] ISocketMessageChannel channel, [CommandParameter(0)] string prefix,
			[CommandParameter(1, false)] bool forServer, [CommandParameter] SocketUser author) {
			EditableSettings(forServer, channel, author).Prefix.SetValue = prefix;

			await Messaging.Instance.SendMessage(channel, $"prefix set to `{prefix}`");
		}
	}
}
