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

		private static void Main() {
			string botToken;
			AsyncThreadsafeFileSyncedDictionary<string, string> settings = FileService.Instance.Settings;
			if (settings.ContainsKey("bot_token")) {
				botToken = settings["bot_token"];
			} else {
				Console.WriteLine("please enter the bot-token:");
				botToken = Console.ReadLine();
				settings["bot_token"] = botToken;
			}
			Client = new Client(botToken);
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
	}
}
