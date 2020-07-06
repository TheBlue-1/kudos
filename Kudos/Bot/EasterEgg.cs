#region
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Kudos.Extensions;
#endregion

namespace Kudos.Bot {
	public sealed class EasterEgg {
		private const string AyayaUrl = "https://media1.tenor.com/images/baf2d324d696b8e0b08daa8cff5c8f12/tenor.gif";

		public static EasterEgg Instance { get; } = new EasterEgg();

		static EasterEgg() { }

		private EasterEgg() { }

		public async Task EasterEggReact(SocketMessage message) {
			if (message.Author.IsBot) {
				return;
			}
			if (message.Content.ToLower().Contains("ayaya")) {
				EmbedBuilder embed = new EmbedBuilder().SetDefaults().WithImageUrl(AyayaUrl).WithDescription(message.Author.Mention + " AYAYA!");
				await message.Channel.SendMessageAsync(embed: embed.Build());
			}
		}
	}
}
