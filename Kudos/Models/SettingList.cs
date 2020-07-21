#region
using System.Collections.Immutable;
using Discord;
using Kudos.Models.bases;
using Newtonsoft.Json;
#endregion

namespace Kudos.Models {
	public abstract class SettingList {
		[JsonProperty]
		protected readonly DictionarySetting<string, string> AutoImage = SettingBase.Create(SettingNames.AutoImage, ImmutableDictionary<string, string>.Empty);
		[JsonProperty]
		protected readonly DictionarySetting<string, string> AutoMessage =
			SettingBase.Create(SettingNames.AutoMessage, ImmutableDictionary<string, string>.Empty);
		[JsonProperty]
		protected readonly DictionarySetting<string, IEmote> AutoReact = SettingBase.Create(SettingNames.AutoReact, ImmutableDictionary<string, IEmote>.Empty);
		[JsonProperty]
		protected readonly Setting<bool> AutoResponses = SettingBase.Create(SettingNames.AutoResponses, true);

		[JsonProperty]
		protected readonly Setting<string> Prefix = SettingBase.Create(SettingNames.Prefix, "k!");
	}
}
