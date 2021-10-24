#region

using DiscordBotsList.Api;
using DiscordBotsList.Api.Objects;
using System.Threading.Tasks;

#endregion

namespace Kudos.Utils {

    public class BotList {
        private static BotList _instance;
        private AuthDiscordBotListApi Api { get; init; }
        public IDblSelfBot ThisBot { get; private set; }

        private BotList() {
        }

        public static async Task<BotList> Instantiate(ulong botId, string topGgToken) {
            if (_instance != null) {
                return _instance;
            }
            _instance = new BotList { Api = new AuthDiscordBotListApi(botId, topGgToken) };
            _instance.ThisBot = await _instance.Api.GetMeAsync();
            return _instance;
        }
    }
}