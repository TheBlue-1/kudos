#region
using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Kudos.Attributes;
using Kudos.DatabaseModels;
using Kudos.Exceptions;
using Kudos.Extensions;
using Kudos.Models;
using Kudos.Utils;

// ReSharper disable UnusedMember.Global
#endregion

namespace Kudos.Bot.Modules {
	[CommandModule("Server Group Calls")]
	public class ServerGroupCalls {
		private DatabaseSyncedList<GroupData> Groups { get; } = new DatabaseSyncedList<GroupData>();

		public static ServerGroupCalls Instance { get; } = new ServerGroupCalls();

		

		static ServerGroupCalls() { }

		private ServerGroupCalls() { }
		[Command("inviteGroup", "sends a dm to all group members to invite them to join your channel")]
		public async Task InviteGroup([CommandParameter] SocketGuildUser user)
		{
			IVoiceChannel channel = user.VoiceChannel;
			if (channel == null)
			{
				throw new KudosInvalidOperationException("you must be in a server audio channel to perform this command");
			}
			GroupData group = Groups.FirstOrDefault(g => g.ChannelId == channel.Id);
			if (group == null)
			{
				throw new KudosInvalidOperationException("this channel has no group (an admin can create one)");
			}
			if (!group.UserIds.Contains(user.Id))
			{
				throw new KudosInvalidOperationException("you are no member of the group in this channel");
			}
			int errorCount = 0;
			foreach (ulong groupUserId in group.UserIds) {
				try { 
				var groupUser=Program.Client.GetSocketUserById(groupUserId);
				var groupUserChannel = await groupUser.GetOrCreateDMChannelAsync();
				
				await Messaging.Instance.SendMessage(groupUserChannel,
					$"Hey, {user.Username} invited you to join the voice call {channel.Name} in {channel.Guild.Name}");
				}
				catch (Exception e) {
					errorCount++;
				}
			}
			throw new KudosUnauthorizedException($"{errorCount} users could not be notified");
		}
		[Command("createGroup", "creates a call group in your current channel", Accessibility.Admin)]
		public void CreateGroup([CommandParameter] SocketGuildUser user, [CommandParameter(0, false)] bool auto)
		{
			IVoiceChannel channel = user.VoiceChannel;
			if (channel == null)
			{
				throw new KudosInvalidOperationException("you must be in a server audio channel to perform this command");
			}
			Groups.Add(new GroupData() { ChannelId = channel.Id, Auto = auto });
		}
		[Command("deleteGroup", "deletes the call group in your current channel", Accessibility.Admin)]
		public void DeleteGroup([CommandParameter] SocketGuildUser user)
		{
			IVoiceChannel channel = user.VoiceChannel;
			if (channel == null)
			{
				throw new KudosInvalidOperationException("you must be in a server audio channel to perform this command");
			}
			GroupData group = Groups.FirstOrDefault(g => g.ChannelId == channel.Id);
			if (group == null)
			{
				throw new KudosInvalidOperationException("you must first create a group in your voice channel");
			}
			Groups.Remove(group);
		}
		[Command("addGroupUser", "adds a user to the current call group", Accessibility.Admin)]
		public void AddUser([CommandParameter] SocketGuildUser user, [CommandParameter(0)]SocketUser addedUser )
		{
			IVoiceChannel channel = user.VoiceChannel;
			if (channel == null)
			{
				throw new KudosInvalidOperationException("you must be in a server audio channel to perform this command");
			}
			GroupData group = Groups.FirstOrDefault(g => g.ChannelId == channel.Id);
			if (group == null)
			{
				throw new KudosInvalidOperationException("you must first create a group in your voice channel");
			}
			group.UserIds.Add(addedUser.Id);
			Groups.Update(group);
		}
		[Command("removeGroupUser", "removes a user to the current call group", Accessibility.Admin)]
		public void RemoveUser([CommandParameter] SocketGuildUser user, [CommandParameter(0)] SocketUser removedUser)
		{
			IVoiceChannel channel = user.VoiceChannel;
			if (channel == null)
			{
				throw new KudosInvalidOperationException("you must be in a server audio channel to perform this command");
			}
			GroupData group = Groups.FirstOrDefault(g => g.ChannelId == channel.Id);
			if (group == null)
			{
				throw new KudosInvalidOperationException("you must first create a group in your voice channel");
			}
			if(!group.UserIds.Contains(removedUser.Id)) {
				throw new KudosInvalidOperationException("the user you want to remove is not in the current call group");
			}

			group.UserIds.Remove(removedUser.Id);
			Groups.Update(group);
		}
	}
}
