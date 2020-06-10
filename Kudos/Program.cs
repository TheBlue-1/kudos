#region
using System;
using System.Threading.Tasks;
using Kudos.Bot;
using Kudos.Utils;
#endregion

namespace Kudos {
	internal class Program {
		private const string Dot = ".";
		private const int WaitingTimeInMs = 250;

		private static void Main(string[] args) {
			string botToken;
			AsyncFileSyncedDictionary<string, string> settings = FileService.Instance.Settings;
			if (settings.ContainsKey("bot_token")) {
				botToken = settings["bot_token"];
			} else {
				Console.WriteLine("please enter the bot-token:");
				botToken = Console.ReadLine();
				settings["bot_token"] = botToken;
			}
			Client client = new Client(botToken);
			while (true) {
				string state = client.State;
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
