#region
using System.Diagnostics.CodeAnalysis;
using Kudos.Models;
#endregion

namespace Kudos.Utils {
	public sealed class SettingsManager {
		private SettingsDictionary GuildSettings { get; } = new SettingsDictionary("guildSettings");

		public static SettingsManager Instance { get; } = new SettingsManager();
		private SettingsDictionary UserSettings { get; } = new SettingsDictionary("userSettings");

		static SettingsManager() { }

		private SettingsManager() { }

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
			Settings settings = new Settings();
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
