#region
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Kudos.Attributes;
using Kudos.DatabaseModels;
using Kudos.Exceptions;
using Kudos.Utils;
#endregion

namespace Kudos.Bot.Modules {
	[CommandModule("Server Group Calls")]
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	public class ServerGroupCalls {
		private DatabaseSyncedList<GroupData> Groups { get; } = new DatabaseSyncedList<GroupData>();

		public static ServerGroupCalls Instance { get; } = new ServerGroupCalls();

		static ServerGroupCalls() { }

		private ServerGroupCalls() { }

		[Command("addgroupuser", "adds a user to the current call group", Accessibility.Admin)]
		public async Task AddUser([CommandParameter] SocketGuildUser user, [CommandParameter(0)] SocketUser addedUser,
			[CommandParameter] SocketTextChannel textChannel) {
			IVoiceChannel channel = user.VoiceChannel;
			if (channel == null) {
				throw new KudosInvalidOperationException("you must be in a server audio channel to perform this command");
			}
			GroupData group = Groups.FirstOrDefault(g => g.ChannelId == channel.Id);
			if (group == null) {
				throw new KudosInvalidOperationException("you must first create a group in your voice channel");
			}
			if (group.UserIds.Contains(addedUser.Id)) {
				throw new KudosInvalidOperationException("the user you want to add is already in the current call group");
			}
			group.UserIds.Add(addedUser.Id);
			Groups.Update(group);
			await Messaging.Instance.SendExpiringMessage(textChannel, "User added successfully");
		}

		[Command("autogroup", "the group members will be notified automatically when one group member enters the channel", Accessibility.Admin)]
		public async Task AutomateGroup([CommandParameter] SocketGuildUser user, [CommandParameter(0, true)] bool auto,
			[CommandParameter] SocketTextChannel textChannel) {
			IVoiceChannel channel = user.VoiceChannel;
			if (channel == null) {
				throw new KudosInvalidOperationException("you must be in a server audio channel to perform this command");
			}
			GroupData group = Groups.FirstOrDefault(g => g.ChannelId == channel.Id);
			if (group == null) {
				throw new KudosInvalidOperationException("you must first create a group in your voice channel");
			}
			group.Auto = auto;
			Groups.Update(group);
			await Messaging.Instance.SendExpiringMessage(textChannel, "Group automated successfully");
		}

		public async Task CheckEntering(SocketUser user, IVoiceChannel channel) {
			GroupData group = Groups.FirstOrDefault(g => g.ChannelId == channel.Id);
			if (group != null && group.Auto && group.UserIds.Contains(user.Id) && await channel.GetUsersAsync().CountAsync() == 1) {
				await SendInvites(group, channel, user);
			}
		}

		[Command("creategroup", "creates a call group in your current channel", Accessibility.Admin)]
		public async Task CreateGroup([CommandParameter] SocketGuildUser user, [CommandParameter(0, false)] bool auto,
			[CommandParameter] SocketTextChannel textChannel) {
			IVoiceChannel channel = user.VoiceChannel;
			if (channel == null) {
				throw new KudosInvalidOperationException("you must be in a server audio channel to perform this command");
			}
			GroupData group = Groups.FirstOrDefault(g => g.ChannelId == channel.Id);
			if (group != null) {
				throw new KudosInvalidOperationException("there is already a group in this channel");
			}
			Groups.Add(new GroupData { ChannelId = channel.Id, Auto = auto });
			await Messaging.Instance.SendExpiringMessage(textChannel, "Group created successfully");
		}

		[Command("deletegroup", "deletes the call group in your current channel", Accessibility.Admin)]
		public async Task DeleteGroup([CommandParameter] SocketGuildUser user, [CommandParameter] SocketTextChannel textChannel) {
			IVoiceChannel channel = user.VoiceChannel;
			if (channel == null) {
				throw new KudosInvalidOperationException("you must be in a server audio channel to perform this command");
			}
			GroupData group = Groups.FirstOrDefault(g => g.ChannelId == channel.Id);
			if (group == null) {
				throw new KudosInvalidOperationException("you must first create a group in your voice channel");
			}
			Groups.Remove(group);
			await Messaging.Instance.SendExpiringMessage(textChannel, "Group deleted successfully");
		}

		[Command("invitegroup", "sends a dm to all group members to invite them to join your channel")]
		public async Task InviteGroup([CommandParameter] SocketGuildUser user, [CommandParameter] SocketTextChannel textChannel) {
			IVoiceChannel channel = user.VoiceChannel;
			if (channel == null) {
				throw new KudosInvalidOperationException("you must be in a server audio channel to perform this command");
			}
			GroupData group = Groups.FirstOrDefault(g => g.ChannelId == channel.Id);
			if (group == null) {
				throw new KudosInvalidOperationException("this channel has no group (an admin can create one)");
			}
			if (!group.UserIds.Contains(user.Id)) {
				throw new KudosInvalidOperationException("you are no member of the group in this channel");
			}
			await SendInvites(group, channel, user);
			await Messaging.Instance.SendExpiringMessage(textChannel, "Group invited successfully");
		}

		[Command("removegroupuser", "removes a user to the current call group", Accessibility.Admin)]
		public async Task RemoveUser([CommandParameter] SocketGuildUser user, [CommandParameter(0)] SocketUser removedUser,
			[CommandParameter] SocketTextChannel textChannel) {
			IVoiceChannel channel = user.VoiceChannel;
			if (channel == null) {
				throw new KudosInvalidOperationException("you must be in a server audio channel to perform this command");
			}
			GroupData group = Groups.FirstOrDefault(g => g.ChannelId == channel.Id);
			if (group == null) {
				throw new KudosInvalidOperationException("you must first create a group in your voice channel");
			}
			if (!group.UserIds.Contains(removedUser.Id)) {
				throw new KudosInvalidOperationException("the user you want to remove is not in the current call group");
			}

			group.UserIds.Remove(removedUser.Id);
			Groups.Update(group);
			await Messaging.Instance.SendExpiringMessage(textChannel, "User removed successfully");
		}

		private static async Task SendInvites(GroupData group, IGuildChannel channel, IUser user) {
			int errorCount = 0;
			foreach (ulong groupUserId in group.UserIds.Where(groupUserId => groupUserId != user.Id)) {
				try {
					SocketUser groupUser = Program.Client.GetSocketUserById(groupUserId);
					IDMChannel groupUserChannel = await groupUser.GetOrCreateDMChannelAsync();

					await Messaging.Instance.SendMessage(groupUserChannel,
						$"Hey, {user.Username} invited you to join the voice call {channel.Name} in {channel.Guild.Name}");
				}
				catch (Exception) {
					errorCount++;
				}
			}
			if (errorCount > 0) {
				throw new KudosUnauthorizedException($"{errorCount} users could not be notified");
			}
		}
	}
}
