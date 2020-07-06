#region
using System.Threading.Tasks;
using DiscordBotsList.Api;
using DiscordBotsList.Api.Objects;
#endregion

namespace Kudos.Utils {
	public class BotList {
		private AuthDiscordBotListApi Api { get; set; }
		public IDblSelfBot ThisBot { get; private set; }

		private BotList() { }

		public static async Task<BotList> Instantiate(ulong botId, string topGgToken) {
			BotList botList = new BotList { Api = new AuthDiscordBotListApi(botId, topGgToken) };
			botList.ThisBot = await botList.Api.GetMeAsync();
			return botList;
		}
	}
}
