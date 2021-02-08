#region
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Kudos.Attributes;
using Kudos.DatabaseModels;
using Kudos.Exceptions;
using Kudos.Extensions;
using Kudos.Utils;
#endregion

namespace Kudos.Bot.Modules {
	[CommandModule("Server Group Calls")]
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	public class ServerGroupCalls {
		private DatabaseSyncedList<GroupData> Groups { get; } = DatabaseSyncedList.Instance<GroupData>();

		public static ServerGroupCalls Instance { get; } = new ServerGroupCalls();
		private Dictionary<ulong, DateTime> Timeouts { get; } = new Dictionary<ulong, DateTime>();

		static ServerGroupCalls() { }

		private ServerGroupCalls() { }

		[Command("addgrouprole", "adds a role to the current call group", Accessibility.Admin)]
		public async Task AddRole([CommandParameter] SocketGuildUser user, [CommandParameter(0)] SocketRole addedRole,
			[CommandParameter] ISocketMessageChannel textChannel) {
			IVoiceChannel channel = user.VoiceChannel;
			if (channel == null) {
				throw new KudosInvalidOperationException("You must be in a server audio channel to perform this command");
			}
			GroupData group = Groups.FirstOrDefault(g => g.ChannelId == channel.Id);
			if (group == null) {
				throw new KudosInvalidOperationException("You must first create a group in your voice channel");
			}
			if (group.RoleIds.Contains(addedRole.Id)) {
				throw new KudosInvalidOperationException("The role you want to add is already in the current call group");
			}
			group.RoleIds.Add(addedRole.Id);
			Groups.Update(group);
			await Messaging.Instance.SendExpiringMessage(textChannel, "Role added successfully");
		}

		[Command("addgroupuser", "adds a user to the current call group", Accessibility.Admin)]
		public async Task AddUser([CommandParameter] SocketGuildUser user, [CommandParameter(0)] SocketUser addedUser,
			[CommandParameter] ISocketMessageChannel textChannel) {
			IVoiceChannel channel = user.VoiceChannel;
			if (channel == null) {
				throw new KudosInvalidOperationException("You must be in a server audio channel to perform this command");
			}
			GroupData group = Groups.FirstOrDefault(g => g.ChannelId == channel.Id);
			if (group == null) {
				throw new KudosInvalidOperationException("You must first create a group in your voice channel");
			}
			if (group.UserIds.Contains(addedUser.Id)) {
				throw new KudosInvalidOperationException("The user you want to add is already in the current call group");
			}
			group.UserIds.Add(addedUser.Id);
			Groups.Update(group);
			await Messaging.Instance.SendExpiringMessage(textChannel, "User added successfully");
		}

		[Command("autogroup", "the group members will be notified automatically when one group member enters the channel", Accessibility.Admin)]
		public async Task AutomateGroup([CommandParameter] SocketGuildUser user, [CommandParameter(0, true)] bool auto,
			[CommandParameter] ISocketMessageChannel textChannel) {
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

		public async Task CheckEntering(SocketUser user, SocketVoiceChannel channel) {
			GroupData group = Groups.FirstOrDefault(g => g.ChannelId == channel.Id);
			if (group == null) {
				return;
			}

			ulong[] roleUserIds = (await RolesUserIds(channel.Guild, group)).ToArray();
			if (group.Auto && (group.UserIds.Contains(user.Id) || roleUserIds.Contains(user.Id))) {
				if ((await UsersInChannel(channel, group, roleUserIds)).Count() == 1) {
					await SendInvites(group, channel, user, roleUserIds);
				}
			}
		}

		public async Task CheckLeaving(SocketUser user, IVoiceChannel channel) {
			GroupData group = Groups.FirstOrDefault(g => g.ChannelId == channel.Id);
			if (group == null) {
				return;
			}

			ulong[] roleUserIds = (await RolesUserIds(channel.Guild, group)).ToArray();
			if (group.Auto && (group.UserIds.Contains(user.Id) || roleUserIds.Contains(user.Id))) {
				if (!(await UsersInChannel(channel, group, roleUserIds)).Any()) {
					Timeouts[group.ChannelId] = DateTime.UtcNow;
				}
			}
		}

		[Command("creategroup", "creates a call group in your current channel", Accessibility.Admin)]
		public async Task CreateGroup([CommandParameter] SocketGuildUser user, [CommandParameter(0, false)] bool auto,
			[CommandParameter] ISocketMessageChannel textChannel) {
			IVoiceChannel channel = user.VoiceChannel;
			if (channel == null) {
				throw new KudosInvalidOperationException("You must be in a server audio channel to perform this command");
			}
			GroupData group = Groups.FirstOrDefault(g => g.ChannelId == channel.Id);
			if (group != null) {
				throw new KudosInvalidOperationException("There is already a group in this channel");
			}
			Groups.Add(new GroupData { ChannelId = channel.Id, Auto = auto });
			await Messaging.Instance.SendExpiringMessage(textChannel, "Group created successfully");
		}

		[Command("deletegroup", "deletes the call group in your current channel", Accessibility.Admin)]
		public async Task DeleteGroup([CommandParameter] SocketGuildUser user, [CommandParameter] ISocketMessageChannel textChannel) {
			IVoiceChannel channel = user.VoiceChannel;
			if (channel == null) {
				throw new KudosInvalidOperationException("You must be in a server audio channel to perform this command");
			}
			GroupData group = Groups.FirstOrDefault(g => g.ChannelId == channel.Id);
			if (group == null) {
				throw new KudosInvalidOperationException("You must first create a group in your voice channel");
			}
			Groups.Remove(group);
			await Messaging.Instance.SendExpiringMessage(textChannel, "Group deleted successfully");
		}

		[Command("invitegroup", "sends a dm to all group members to invite them to join your channel")]
		public async Task InviteGroup([CommandParameter] SocketGuildUser user, [CommandParameter] ISocketMessageChannel textChannel) {
			SocketVoiceChannel channel = user.VoiceChannel;
			if (channel == null) {
				throw new KudosInvalidOperationException("You must be in a server audio channel to perform this command");
			}
			GroupData group = Groups.FirstOrDefault(g => g.ChannelId == channel.Id);
			if (group == null) {
				throw new KudosInvalidOperationException("This channel has no group (an admin can create one)");
			}
			ulong[] roleUserIds = (await RolesUserIds(channel.Guild, group)).ToArray();
			if (!group.UserIds.Contains(user.Id) && !roleUserIds.Contains(user.Id)) {
				throw new KudosInvalidOperationException("You are no member of the group in this channel");
			}
			await SendInvites(group, channel, user, roleUserIds);
			await Messaging.Instance.SendExpiringMessage(textChannel, "Group invited successfully");
		}

		[Command("removegrouprole", "removes a role from the current call group", Accessibility.Admin)]
		public async Task RemoveRole([CommandParameter] SocketGuildUser user, [CommandParameter(0)] SocketRole removedRole,
			[CommandParameter] ISocketMessageChannel textChannel) {
			IVoiceChannel channel = user.VoiceChannel;
			if (channel == null) {
				throw new KudosInvalidOperationException("You must be in a server audio channel to perform this command");
			}
			GroupData group = Groups.FirstOrDefault(g => g.ChannelId == channel.Id);
			if (group == null) {
				throw new KudosInvalidOperationException("You must first create a group in your voice channel");
			}
			if (!group.RoleIds.Contains(removedRole.Id)) {
				throw new KudosInvalidOperationException("The role you want to remove is not in the current call group");
			}

			group.RoleIds.Remove(removedRole.Id);
			Groups.Update(group);
			await Messaging.Instance.SendExpiringMessage(textChannel, "Role removed successfully");
		}

		[Command("removegroupuser", "removes a user from the current call group", Accessibility.Admin)]
		public async Task RemoveUser([CommandParameter] SocketGuildUser user, [CommandParameter(0)] SocketUser removedUser,
			[CommandParameter] ISocketMessageChannel textChannel) {
			IVoiceChannel channel = user.VoiceChannel;
			if (channel == null) {
				throw new KudosInvalidOperationException("You must be in a server audio channel to perform this command");
			}
			GroupData group = Groups.FirstOrDefault(g => g.ChannelId == channel.Id);
			if (group == null) {
				throw new KudosInvalidOperationException("You must first create a group in your voice channel");
			}
			if (!group.UserIds.Contains(removedUser.Id)) {
				throw new KudosInvalidOperationException("The user you want to remove is not in the current call group");
			}

			group.UserIds.Remove(removedUser.Id);
			Groups.Update(group);
			await Messaging.Instance.SendExpiringMessage(textChannel, "User removed successfully");
		}

		private static async Task<IEnumerable<ulong>> RolesUserIds(IGuild guild, GroupData group) {
			return (await guild.GetUsersAsync()).Where(guildUser => guildUser.HasRoleId(group.RoleIds.ToArray())).Select(guildUser => guildUser.Id);
		}

		private async Task SendInvites(GroupData group, SocketVoiceChannel channel, IUser user, ulong[] roleUserIds) {
			if (Timeouts.ContainsKey(group.ChannelId)) {
				TimeSpan timeout = Timeouts[group.ChannelId] - DateTime.UtcNow.AddMinutes(-5);
				if (timeout > TimeSpan.Zero) {
					throw new KudosInvalidOperationException($"There is still a Timeout for {timeout}");
				}
			}
			int errorCount = 0;
			HashSet<ulong> userIds = new HashSet<ulong>();
			userIds.UnionWith(group.UserIds);
			userIds.UnionWith(roleUserIds);
			IEnumerable<IGuildUser> alreadyInChannel = await UsersInChannel(channel, group, roleUserIds);
			foreach (ulong groupUserId in userIds.Where(groupUserId => alreadyInChannel.All(channelUser => channelUser.Id != groupUserId))) {
				try {
					SocketUser groupUser = Program.Client.GetSocketUserById(groupUserId);
					IDMChannel groupUserChannel = await groupUser.GetOrCreateDMChannelAsync();

					IReadOnlyCollection<IInviteMetadata> invites = await channel.GetInvitesAsync();
					IInviteMetadata invite = invites.FirstOrDefault(i =>
							i.ChannelId == channel.Id && i.IsTemporary == false && i.Inviter.Id == Program.Client.BotUserId)
						?? await channel.CreateInviteAsync();
					await Messaging.Instance.SendMessage(groupUserChannel,
						$"Hey, **{user.Username}** invited you to join the voice call [**{channel.Name}** in {channel.Guild.Name}]({invite.Url})");
				}
				catch (Exception) {
					errorCount++;
				}
			}
			if (errorCount > 0) {
				throw new KudosUnauthorizedException($"{errorCount} users could not be notified");
			}

			Timeouts[group.ChannelId] = DateTime.UtcNow;
		}

		private static async Task<IEnumerable<IGuildUser>> UsersInChannel(IGuildChannel channel, GroupData group, IEnumerable<ulong> roleUserIds) {
			return (await channel.GetUsersAsync().AwaitAll()).Where(channelUser =>
				group.UserIds.Contains(channelUser.Id) || roleUserIds.Contains(channelUser.Id));
		}
	}
}
