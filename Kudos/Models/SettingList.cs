#region
using System.Collections.Immutable;
using Discord;
using Kudos.Models.bases;
using Newtonsoft.Json;
#endregion

namespace Kudos.Models {
	public abstract class SettingList {
		[JsonProperty]
		protected readonly ListSetting<string> AutoHonor = SettingBase.Create(SettingNames.AutoHonor, ImmutableHashSet<string>.Empty,
			"Reacts with a honor message (honor without command) when someone writes [value] and mentions a user");
		[JsonProperty]
		protected readonly DictionarySetting<string, string> AutoImage = SettingBase.Create(SettingNames.AutoImage, ImmutableDictionary<string, string>.Empty,
			"Sends image with url [value] when someone writes [key]");
		[JsonProperty]
		protected readonly DictionarySetting<string, string> AutoMessage = SettingBase.Create(SettingNames.AutoMessage,
			ImmutableDictionary<string, string>.Empty, "Sends message with text [value] when someone writes [key]");
		[JsonProperty]
		protected readonly DictionarySetting<string, IEmote> AutoReact = SettingBase.Create(SettingNames.AutoReact, ImmutableDictionary<string, IEmote>.Empty,
			"Reacts with Emote [value] when someone writes [key]");
		[JsonProperty]
		protected readonly Setting<bool> AutoResponses = SettingBase.Create(SettingNames.AutoResponses, true, "activates/deactivates automatic responses");
		[JsonProperty]
		protected readonly Setting<IMessageChannel> HonorChannel = SettingBase.Create(SettingNames.HonorChannel, (IMessageChannel)null,
			"a channel where all honor transactions of your guild will be displayed with the according message (only for guilds)");

		[JsonProperty]
		protected readonly Setting<string> Prefix = SettingBase.Create(SettingNames.Prefix, "k!", "sets a new prefix");

		[JsonProperty]
		protected readonly Setting<Timezone> Timezone =
			SettingBase.Create(SettingNames.Timezone, (Timezone)0, "sets your timezone to interpret times correctly");
	}
}
