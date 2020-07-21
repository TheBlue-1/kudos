#region
using System.Collections.Immutable;
using Discord;
using Kudos.Models.bases;
using Newtonsoft.Json;
#endregion

namespace Kudos.Models {
	public abstract class SettingList {
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
		protected readonly Setting<string> Prefix = SettingBase.Create(SettingNames.Prefix, "k!", "sets a new prefix");
	}
}
