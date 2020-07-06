#region
using System;
using System.Reflection;
using System.Threading.Tasks;
using Kudos.Bot;
using Kudos.Utils;
#endregion

namespace Kudos {
	internal class Program {
		private const string Dot = ".";
		private const int WaitingTimeInMs = 250;
		public static Random Random { get; } = new Random();

		public static Version Version { get; } = Assembly.GetExecutingAssembly().GetName().Version;

		public static Client Client { get; private set; }

		public static bool IsBotListBot { get; private set; }
		public static string BotListToken { get; private set; }
		public static BotList BotList { get; private set; }
		public const ulong BotListBotId = 719571683517792286;

	#if DEBUG
		public const bool Debug = true;
	#else
		public const bool Debug = false;
	#endif

		private static void Main() {
			string botToken;
			AsyncThreadsafeFileSyncedDictionary<string, string> settings = FileService.Instance.Settings;
			if (settings.ContainsKey("bot_list_token")) {
				BotListToken = settings["bot_list_token"];
			}
			if (settings.ContainsKey("is_bot_list_bot")) {
				IsBotListBot = bool.Parse(settings["is_bot_list_bot"]);
			}
			if (settings.ContainsKey("bot_token")) {
				botToken = settings["bot_token"];
			} else {
				Console.WriteLine("please enter the bot-token:");
				botToken = Console.ReadLine();
				settings["bot_token"] = botToken;
			}
			Client = new Client(botToken);
			RefreshBotListDocs();
			while (true) {
				string state = Client.State;
				Console.Write(state);
				Task.Delay(WaitingTimeInMs).Wait();
				Console.Write(Dot);
				Task.Delay(WaitingTimeInMs).Wait();
				Console.Write(Dot);
				Task.Delay(WaitingTimeInMs).Wait();
				Console.Write(Dot);
				Task.Delay(WaitingTimeInMs).Wait();
				Console.Clear();
			}

			// ReSharper disable once FunctionNeverReturns
		}

		private static async void RefreshBotListDocs() {
			if (BotListToken == null) {
				return;
			}

			BotList = await BotList.Instantiate(BotListBotId, BotListToken);

			if (!IsBotListBot) {
				return;
			}

			string html = CommandModules.Instance.LongDescription;

			FileService.Instance.WriteFile("description.html", html);

			Client.JoinedNewGuild += () => { BotList.ThisBot.UpdateStatsAsync(Client.Guilds.Count); };
		}
	}
}
