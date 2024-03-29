﻿#region

using Google.Cloud.Logging.Type;
using Kudos.Bot;
using Kudos.DatabaseModels;
using Kudos.Extensions;
using Kudos.Utils;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

#endregion

namespace Kudos {

    internal static class Program {
        private const int WaitingTimeInMs = 10000;
        public static Random Random { get; } = new();

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
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;
            AppDomain.CurrentDomain.ProcessExit += OnClose;
            KudosDataContext db = new();
            db.Database.Migrate();

            string botToken;

            string html = new HtmlGenerator().LongDescription();

            FileService.Instance.WriteFile("description.html", html);
            AsyncThreadsafeFileSyncedDictionary<string, string> settings = FileService.Instance.Settings;

            if (settings.ContainsKey("bot_list_token")) {
                BotListToken = settings["bot_list_token"];
            }
            if (settings.ContainsKey("is_bot_list_bot")) {
                IsBotListBot = bool.Parse(settings["is_bot_list_bot"]);
            }
            if (settings.ContainsKey("bot_token") && !string.IsNullOrEmpty(settings["bot_token"])) {
                botToken = settings["bot_token"];
            } else {
                Console.WriteLine("please enter the bot-token:");
                botToken = Console.ReadLine();
                settings["bot_token"] = botToken;
            }
            Client = new Client(botToken);

            Client.StateChanged += ClientStateChanged;

            LogService.Instance.Log(DateTime.UtcNow + ": start initiated", LogService.LogType.Running, LogSeverity.Info);
            Client.Start();
            RefreshBotListDocs(botToken).RunAsyncSave();
            while (true) {
                Task.Delay(WaitingTimeInMs).Wait();
            }

            // ReSharper disable once FunctionNeverReturns
        }

        private static void UnhandledException(object sender, UnhandledExceptionEventArgs e) {
            LogService.Instance.Log($"Unhandled Exception: '{(e.ExceptionObject is Exception ex ? ex.Message : "no message found")}' ({e.ExceptionObject})",
                LogService.LogType.Main, LogSeverity.Critical, false);
        }

        private static void ClientStateChanged(object sender, Client.StateChangedData e) {
            LogService.Instance.Log(DateTime.UtcNow + ": " + e, LogService.LogType.Running, LogSeverity.Info);
            Console.WriteLine(DateTime.UtcNow + ": " + e);
        }

        private static void OnClose(object sender, EventArgs e) {
            LogService.Instance.Log("#################APP SHUTS DOWN#################", LogService.LogType.Running, LogSeverity.Notice, false);
            Console.WriteLine("#################APP SHUTS DOWN#################");
        }

        private static async Task RefreshBotListDocs(string botToken) {
            if (BotListToken == null) {
                return;
            }

            BotList = await BotList.Instantiate(BotListBotId, BotListToken);

            if (!IsBotListBot) {
                return;
            }

            Client.GuildCountChanged += () => { BotList.ThisBot.UpdateStatsAsync(new[] { Client.Guilds.Count }); };

            string html = new HtmlGenerator().LongDescription();

            HttpClient client = new(new HttpClientHandler { AllowAutoRedirect = true });
            Dictionary<string, string> data = new() { { "password", botToken }, { "username", "Kudos" }, { "html", html } };
            await client.PostAsync("https://kudos.ml/update/", new FormUrlEncodedContent(data));
        }
    }
}