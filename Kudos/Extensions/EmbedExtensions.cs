#region
using Discord;
#endregion

namespace Kudos.Extensions {
	public static class EmbedExtensions {
		public static EmbedBuilder SetDefaults(this EmbedBuilder builder) {
			const string authorName = "Kudos";
			string footerText = $"Kudos {Program.Version} © by TheBlueOne & H3XO3ZE";
			const string iconUrl =
				"https://images.discordapp.net/avatars/719571683517792286/ef9ecf361489e5e1cfe2cd20561516d1.png?size=512"; //todo change url to bot logo when accepted (maybe also from bot gg)
			const string url = "https://top.gg/bot/719571683517792286"; //todo change url to top gg bot site when accepted
			const uint color = 35823; //0 137 239 (binary)
			builder.WithAuthor(authorName, iconUrl, url).WithColor(color).WithFooter(footerText, iconUrl).WithCurrentTimestamp().WithUrl(url);
			return builder;
		}
	}
}
