#region
using System;
using Discord.WebSocket;
using Kudos.Utils;
#endregion

namespace Kudos.Bot {
	public sealed class Honor {
		private const byte MaxHonorPerDay = 7;
		private static readonly string[] HonorFeedbackHigh = {
			"You gained some honor... Keep it up!", "“I would prefer even to fail with honor than win by cheating.” Sophocles",
			"You officially fulfill the conditions to become a Medal-Of-Honor candidate"
		};
		private static readonly string[] HonorFeedbackHigher = {
			"“You will never do anything in this world without courage. It is the greatest quality of the mind next to honor.” Aristotle",
			"You can call yourself 2nd Tier Ehrenmann now", "You are an important part of our small community <3"
		};
		private static readonly string[] HonorFeedbackHighest = { "YOU ARE A TRUE PERSON OF HONOR!", "You truly deserve a medal of Honor", "Marry me pls uwu" };
		private static readonly string[] HonorFeedbackLow = {
			"Sub-Zero Honor... How does that feel?", "It looks like you have enemies on this server...", "Oh no what happened? Your Honor is fainting...",
			"Maybe you should think about being nicer to people?"
		};
		private static readonly string[] HonorFeedbackLower = {
			"Whoah dude you really crossed a line there...", "What have you done to deserve this?", "Your Karma is astonishingly low!",
			"Not good you reputation is! -Master Yoda"
		};

		private static readonly string[] HonorFeedbackLowest = {
			"People really hate you don't they?", "Oh boy you must have done something annoying!", "Watch out we have a real Mr. Trump here!",
			"See you in hell buddy!", "|| https://www.youtube.com/watch?v=Poz4SQJTWsE&list=RDAMVMApHC5YWo1Rc ||"
		};
		private AsyncFileSyncedDictionary<ulong, int> BalancesPerId { get; } = new AsyncFileSyncedDictionary<ulong, int>("balances");
		public static Honor Instance { get; } = new Honor();
		private AsyncFileSyncedDictionary<ulong, int> UsedHonor { get; } =
			new AsyncFileSyncedDictionary<ulong, int>("honorUsage" + DateTime.Now.Date.ToShortDateString());

		static Honor() { }

		private Honor() { }

		private void ChangeUsersUsedHonor(ulong userId, int count) {
			int honoringBalance = UsedHonor.ContainsKey(userId) ? UsedHonor[userId] : 0;
			UsedHonor[userId] = honoringBalance + count;
		}

		public void DishonorUser(SocketUser honoredUser, SocketUser honoringUser, int count, ISocketMessageChannel channel) {
			count = HonorCount(honoredUser, honoringUser, count, channel);
			if (count == -1) {
				return;
			}
			HonorUser(honoredUser.Id, -count);
			ChangeUsersUsedHonor(honoringUser.Id, count);

			Messaging.Instance.Message(channel, $"You successfully removed ***{count}*** honor points for ***{honoredUser.Mention}***!");
		}

		public int HonorCount(SocketUser honoredUser, SocketUser honoringUser, int count, ISocketMessageChannel channel) {
			if (honoredUser == null || honoringUser.Id == honoredUser.Id) {
				return -1;
			}

			int usedHonor = 0;
			if (UsedHonor.ContainsKey(honoringUser.Id)) {
				usedHonor = UsedHonor[honoringUser.Id];
			}

			count = count >= MaxHonorPerDay - usedHonor ? MaxHonorPerDay - usedHonor : count <= 0 ? 1 : count;
			if (count == 0) {
				Messaging.Instance.Message(channel, $"You already used all ***{MaxHonorPerDay}*** honor points you can give per day!");
				return -1;
			}
			return count;
		}

		public void HonorUser(SocketUser honoredUser, SocketUser honoringUser, int count, ISocketMessageChannel channel) {
			count = HonorCount(honoredUser, honoringUser, count, channel);
			if (count == -1) {
				return;
			}
			HonorUser(honoredUser.Id, count);
			ChangeUsersUsedHonor(honoringUser.Id, count);

			Messaging.Instance.Message(channel, $"You honored ***{honoredUser.Mention}*** with ***{count}*** Points!");
		}

		private void HonorUser(ulong userId, int count) {
			int honorBalance = BalancesPerId.ContainsKey(userId) ? BalancesPerId[userId] : 0;
			BalancesPerId[userId] = honorBalance + count;
		}

		public void SendHonorBalance(SocketUser user, ISocketMessageChannel channel) {
			int honor = BalancesPerId.ContainsKey(user.Id) ? BalancesPerId[user.Id] : 0;
			string honorMessage;

			if (honor < -100) {
				int index = Program.Random.Next(0, HonorFeedbackLowest.Length);
				honorMessage = HonorFeedbackLowest[index];
			} else if (honor >= -100 && honor < -50) {
				int index = Program.Random.Next(0, HonorFeedbackLower.Length);
				honorMessage = HonorFeedbackLower[index];
			} else if (honor >= -50 && honor < 0) {
				int index = Program.Random.Next(0, HonorFeedbackLow.Length);
				honorMessage = HonorFeedbackLow[index];
			} else if (honor >= 0 && honor < 50) {
				int index = Program.Random.Next(0, HonorFeedbackHigh.Length);
				honorMessage = HonorFeedbackHigh[index];
			} else if (honor >= 50 && honor <= 100) {
				int index = Program.Random.Next(0, HonorFeedbackHigher.Length);
				honorMessage = HonorFeedbackHigher[index];
			} else {
				int index = Program.Random.Next(0, HonorFeedbackHighest.Length);
				honorMessage = HonorFeedbackHighest[index];
			}

			Messaging.Instance.Message(channel, $"***{user.Mention}*** has ***{honor}*** honor points. \n***{honorMessage}***");
		}
	}
}
