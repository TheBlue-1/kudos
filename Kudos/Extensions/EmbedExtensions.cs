#region
using Discord;
#endregion

namespace Kudos.Extensions {
	public static class EmbedExtensions {
		public static EmbedBuilder SetDefaults(this EmbedBuilder builder) {
			const string authorName = "Kudos";
			string footerText = $"Kudos {Program.Version} © by TheBlueOne & H3XO3ZE";
			const string iconUrl =
				"https://images.discordapp.net/avatars/719571683517792286/9301d1808e275e18012330f53afdc1f0.png?size=256"; 
			const string url = "https://top.gg/bot/719571683517792286";
			const uint color = 35823; //0 137 239 (binary)
			builder.WithAuthor(authorName, iconUrl, url).WithColor(color).WithFooter(footerText, iconUrl).WithCurrentTimestamp().WithUrl(url);
			return builder;
		}
	}
}
