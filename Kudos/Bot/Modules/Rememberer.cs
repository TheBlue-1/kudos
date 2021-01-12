#region
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Kudos.Attributes;
using Kudos.DatabaseModels;
using Kudos.Exceptions;
using Kudos.Extensions;
using Kudos.Utils;

// ReSharper disable UnusedMember.Global
#endregion

namespace Kudos.Bot.Modules {
	[CommandModule("Rememberer")]
	public sealed class Rememberer {
		public static Rememberer Instance { get; } = new Rememberer();

		private List<Timer> RunningTimers { get; } = new List<Timer>();

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
			if (add) {
				TimerData.Add(timerData);
			}
			Timer timer = new Timer(timerData);
			timer.TimerDead += RemoveTimer;
			timer.TimerEvent += SendRememberer;
			timer.TimerDataChanged += TimerDataChanged;
			RunningTimers.Add(timer);
			timer.Start();
		}

		[Command("delreminder", "delete a reminder by id from reminders")]
		public async Task DeleteReminder([CommandParameter] SocketUser author, [CommandParameter] SocketTextChannel channel,
			[CommandParameter(0)] string reminderId) {
			TimerData timerData = TimerData.FirstOrDefault(timer => timer.Id == reminderId);
			if (timerData == null) {
				throw new KudosArgumentException("Id doesn't exist");
			}
			if (timerData.OwnerId != author.Id) {
				throw new KudosArgumentException(
					$"This reminder doesn't belong to you. It's {(await Program.Client.GetRestUserById(timerData.OwnerId)).Mention}s.");
			}

			RunningTimers.FirstOrDefault(timer => timer.Id == reminderId)?.Kill();
			await Messaging.Instance.SendExpiringMessage(channel, "reminder deleted");
		}

		[Command("remember", "sets a reminder, can be repeated")]
		public async Task Remember([CommandParameter] SocketTextChannel channel, [CommandParameter] SocketUser author, [CommandParameter(0)] DateTime end,
			[CommandParameter(1)] string message, [CommandParameter(2, null)] IMessageChannel messageChannel, [CommandParameter(3, 0)] TimeSpan repeat) {
			TimeSpan waitingTime = end - DateTime.Now;
			if (waitingTime < TimeSpan.Zero) {
				throw new KudosArgumentException("End time must be in the future");
			}
			TimerData timerData = new TimerData {
				OwnerId = author.Id, ChannelId = messageChannel?.Id ?? channel.Id, End = end, Message = message, Repeat = repeat
			};
			CreateTimer(timerData);

			await Messaging.Instance.SendExpiringMessage(channel, $"You will be remembered in {waitingTime.Readable()}", new TimeSpan(0, 0, 5));
		}

		[Command("reminders", "list of all your reminders")]
		public async Task ReminderList([CommandParameter] SocketUser author, [CommandParameter] SocketTextChannel channel) {
			IOrderedEnumerable<TimerData> reminders = TimerData.Where(timer => timer.OwnerId == author.Id).OrderBy(timer => timer.End);
			string reminderList = reminders.Aggregate(string.Empty,
				(current, timerData) =>
					current
					+ $"**[{timerData.Id}] {timerData.End} {(timerData.Repeat > TimeSpan.Zero ? $"(repeats: {timerData.Repeat.LikeInput()})" : "")}**: {timerData.Message}\n");
			await Messaging.Instance.SendMessage(channel, reminderList);
		}

		private void RemoveTimer(object sender, TimerData data) {
			TimerData.Remove(data);
		}

		private static void SendRememberer(object sender, TimerData data) {
			new Func<Task>(async () => {
				IMessageChannel channel = Program.Client.GetMessageChannelById(data.ChannelId);
				await Messaging.Instance.SendMessage(channel, "**Reminder: **" + data.Message);
			}).RunAsyncSave();
		}

		private void TimerDataChanged(object sender, TimerData data) {
			TimerData timerData = TimerData.FirstOrDefault(timer => timer.Id == data.Id);

			if (timerData != null) {
				TimerData.Update(data);
			}
		}
	}
}
