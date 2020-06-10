#region
using System;
using Discord.WebSocket;
using Kudos.Utils;
#endregion

namespace Kudos.Bot {
	public sealed class Honor {
		private const byte MaxHonorPerDay = 5;
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

			Messaging.Instance.Message(channel, $"you successfully removed {count} honor points for {honoredUser.Mention}");
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
				Messaging.Instance.Message(channel, $"you already used all {MaxHonorPerDay} honor points you can give per day");
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

			Messaging.Instance.Message(channel, $"you successfully added {count} honor points for {honoredUser.Mention}");
		}

		private void HonorUser(ulong userId, int count) {
			int honorBalance = BalancesPerId.ContainsKey(userId) ? BalancesPerId[userId] : 0;
			BalancesPerId[userId] = honorBalance + count;
		}

		public void SendHonorBalance(SocketUser user, ISocketMessageChannel channel) {
			int honor = BalancesPerId.ContainsKey(user.Id) ? BalancesPerId[user.Id] : 0;
			Messaging.Instance.Message(channel, $"{user.Mention} has {honor} honor points.");
		}
	}
}
