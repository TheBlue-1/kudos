#region
using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Kudos.Attributes;
using Kudos.DatabaseModels;
using Kudos.Extensions;
using Kudos.Utils;

// ReSharper disable UnusedMember.Global
#endregion

namespace Kudos.Bot.Modules {
	[CommandModule("Rememberer")]
	public sealed class Rememberer {
		public static Rememberer Instance { get; } = new Rememberer();

		private DatabaseSyncedList<TimerData> TimerData { get; } = new DatabaseSyncedList<TimerData>();

		static Rememberer() { }

		private Rememberer() {
			new Action(() => {
				foreach (TimerData timerData in TimerData) {
					CreateTimer(timerData, false);
				}
			}).RunAsyncSave();
		}

		private void CreateTimer(TimerData timerData, bool add = true) {
			if(add)
			TimerData.Add(timerData);
			Timer timer = new Timer(timerData);
			timer.TimerDead += RemoveTimer;
			timer.TimerEvent += SendRememberer;
			timer.Start();
		}

		[Command("remember", "remembers")]
		public async Task Remember([CommandParameter] SocketTextChannel channel, [CommandParameter(0)] DateTime end, [CommandParameter(1)] string message,
			[CommandParameter(2, null)] IMessageChannel messageChannel, [CommandParameter(3, 0)] TimeSpan repeat) {
			TimerData timerData = new TimerData { ChannelId = messageChannel?.Id ?? channel.Id, End = end, Message = message, Repeat = repeat };
			CreateTimer(timerData);
			await Messaging.Instance.SendExpiringMessage(channel, $"You will be remembered in {end-DateTime.Now}");
		}

		private void RemoveTimer(object sender, TimerData data) {
			TimerData.Remove(data);
		}

		private void SendRememberer(object sender, TimerData data) {
			new Func<Task>(async () => {
				IMessageChannel channel = Program.Client.GetMessageChannelById(data.ChannelId);
				await Messaging.Instance.SendMessage(channel, data.Message);
			}).RunAsyncSave();
		}
	}
}
