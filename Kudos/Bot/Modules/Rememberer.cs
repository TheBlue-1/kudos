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
		public static Rememberer Instance { get; } = new();
		private ulong NextId {
			get {
				for (ulong i = 0; i < ulong.MaxValue; i++) {
					if (TimerData.All(timer => timer.Id != i)) {
						return i;
					}
				}

				throw new Exception("ulong id limit exceeded");
			}
		}

		private List<Timer> RunningTimers { get; } = new();

		private DatabaseSyncedList<TimerData> TimerData { get; } = DatabaseSyncedList.Instance<TimerData>();

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
			Timer timer = new(timerData);
			timer.TimerDead += RemoveTimer;
			timer.TimerEvent += SendRememberer;
			timer.TimerDataChanged += TimerDataChanged;
			timer.SkippedTimerEvent += SendLateRememberer;
			RunningTimers.Add(timer);
			timer.Start();
		}

		[Command("delreminder", "delete a reminder by id from reminders", Accessibility.Admin)]
		public async Task DeleteReminder([CommandParameter] SocketUser author, [CommandParameter] ISocketMessageChannel channel,
			[CommandParameter(0)] ulong reminderId) {
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

		[Command("remember", "sets a reminder, can be repeated", Accessibility.Admin)]
		public async Task Remember([CommandParameter] ISocketMessageChannel channel, [CommandParameter] SocketUser author, [CommandParameter(0)] DateTime end,
			[CommandParameter(1)] string message, [CommandParameter(2, null)] IMessageChannel messageChannel, [CommandParameter(3, 0)] TimeSpan repeat) {
			TimeSpan waitingTime = end - DateTime.UtcNow;
			if (waitingTime < TimeSpan.Zero) {
				throw new KudosArgumentException("End time must be in the future");
			}
			if (repeat > TimeSpan.Zero && repeat < new TimeSpan(0, 5, 0)) {
				throw new KudosArgumentException("the minimum repeat time are 300 seconds/5 minutes");
			}
			if (TimerData.Count(t => t.OwnerId == author.Id) >= 10) {
				throw new KudosInvalidOperationException(
					"You have reached the current limit of 10 reminders per person. If you have a valid reason to use more than that please get in touch with our support team on our Support server.");
			}
			TimerData timerData = new() {
				OwnerId = author.Id, ChannelId = messageChannel?.Id ?? channel.Id, End = end, Message = message, Repeat = repeat, Id = NextId
			};
			CreateTimer(timerData);

			await Messaging.Instance.SendExpiringMessage(channel, $"You will be remembered in {waitingTime.Readable()}", new TimeSpan(0, 0, 5));
		}

		[Command("reminders", "list of all your reminders")]
		public async Task ReminderList([CommandParameter] SocketUser author, [CommandParameter] ISocketMessageChannel channel) {
			IOrderedEnumerable<TimerData> reminders = TimerData.Where(timer => timer.OwnerId == author.Id).OrderBy(timer => timer.End);
			string reminderList = reminders.Aggregate(string.Empty,
				(current, timerData) =>
					current
					+ $"**[{timerData.Id}] {timerData.End}(UTC) {(timerData.Repeat > TimeSpan.Zero ? $"(repeats: {timerData.Repeat.LikeInput()})" : "")}**: {timerData.Message}\n");
			await Messaging.Instance.SendMessage(channel, reminderList);
		}

		private void RemoveTimer(object sender, TimerData data) {
			TimerData.Remove(data);
			RunningTimers.Remove(RunningTimers.First(timer => timer.Id == data.Id));
		}

		private static void SendLateRememberer(object sender, TimerData data) {
			new Func<Task>(async () => {
				IMessageChannel channel = Program.Client.GetMessageChannelById(data.ChannelId);
				await Messaging.Instance.SendMessage(channel,
					"Found a skipped Reminder (maybe Kudos was down at the remind time) \n **Reminder: **" + data.Message);
			}).RunAsyncSave();
		}

		private static void SendRememberer(object sender, TimerData data) {
			new Func<Task>(async () => {
				IMessageChannel channel = Program.Client.GetMessageChannelById(data.ChannelId);
				if (data.Message.StartsWith('=')) {
					await Messaging.Instance.SendMessage(channel, "=**Reminder: **" + data.Message.Substring(1));
				} else {
					await Messaging.Instance.SendMessage(channel, "**Reminder: **" + data.Message);
				}
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
