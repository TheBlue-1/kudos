#region
using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Kudos.Attributes;
using Kudos.Exceptions;
using Kudos.Utils;

// ReSharper disable UnusedMember.Global
#endregion

namespace Kudos.Bot.Modules {
	[CommandModule("Honor")]
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
			"Whoa dude you really crossed a line there...", "What have you done to deserve this?", "Your Karma is astonishingly low!",
			"Not good you reputation is! -Master Yoda"
		};

		private static readonly string[] HonorFeedbackLowest = {
			"People really hate you don't they?", "Oh boy you must have done something annoying!", "Watch out we have a real Mr. Trump here!",
			"See you in hell buddy!", "|| https://www.youtube.com/watch?v=Poz4SQJTWsE&list=RDAMVMApHC5YWo1Rc ||"
		};

		private AsyncThreadsafeFileSyncedDictionary<ulong, int> BalancesPerId { get; } = new AsyncThreadsafeFileSyncedDictionary<ulong, int>("balances");
		public static Honor Instance { get; } = new Honor();
		private AsyncThreadsafeFileSyncedDictionary<ulong, int> UsedHonor { get; } =
			new AsyncThreadsafeFileSyncedDictionary<ulong, int>("honorUsage" + DateTime.Now.Date.ToShortDateString());

		static Honor() { }

		private Honor() { }

		private void ChangeUsersUsedHonor(ulong userId, int count) {
			int honoringBalance = UsedHonor.ContainsKey(userId) ? UsedHonor[userId] : 0;
			UsedHonor[userId] = honoringBalance + count;
		}

		[Command("dishonor")]
		public async Task DishonorUser([CommandParameter(1)] SocketUser honoredUser, [CommandParameter] SocketUser honoringUser,
			[CommandParameter(0, 1)] int count, [CommandParameter] ISocketMessageChannel channel) {
			count = HonorCount(honoredUser, honoringUser, count);

			HonorUser(honoredUser.Id, -count);
			ChangeUsersUsedHonor(honoringUser.Id, count);

			await Messaging.Instance.SendMessage(channel, $"You successfully removed ***{count}*** honor points for ***{honoredUser.Mention}***!");
		}

		public int HonorCount(SocketUser honoredUser, SocketUser honoringUser, int count) {
			if (honoringUser.Id == honoredUser.Id) {
				throw new KudosUnauthorizedException("You are not allowed to honor yourself");
			}

			int usedHonor = 0;
			if (UsedHonor.ContainsKey(honoringUser.Id)) {
				usedHonor = UsedHonor[honoringUser.Id];
			}

			count = count >= MaxHonorPerDay - usedHonor ? MaxHonorPerDay - usedHonor : count <= 0 ? 1 : count;
			if (count == 0) {
				throw new KudosUnauthorizedException("You already used all your daily honor points");
			}
			return count;
		}

		[Command("honor")]
		public async Task HonorUser([CommandParameter(1)] SocketUser honoredUser, [CommandParameter] SocketUser honoringUser,
			[CommandParameter(0, 1)] int count, [CommandParameter] ISocketMessageChannel channel) {
			count = HonorCount(honoredUser, honoringUser, count);

			HonorUser(honoredUser.Id, count);
			ChangeUsersUsedHonor(honoringUser.Id, count);

			await Messaging.Instance.SendMessage(channel, $"You honored ***{honoredUser.Mention}*** with ***{count}*** Points!");
		}

		private void HonorUser(ulong userId, int count) {
			int honorBalance = BalancesPerId.ContainsKey(userId) ? BalancesPerId[userId] : 0;
			BalancesPerId[userId] = honorBalance + count;
		}

		[Command("balance")]
		public async Task SendHonorBalance([CommandParameter(0, CommandParameter.SpecialDefaults.IndexLess)]
			SocketUser user, [CommandParameter] ISocketMessageChannel channel) {
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

			await Messaging.Instance.SendMessage(channel, $"***{user.Mention}*** has ***{honor}*** honor points. \n***{honorMessage}***");
		}
	}
}
