#region

using Discord;
using Discord.WebSocket;
using Kudos.Attributes;
using Kudos.DatabaseModels;
using Kudos.Exceptions;
using Kudos.Extensions;
using Kudos.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

#endregion

namespace Kudos.Bot.Modules {

    [CommandModule("Server Group Calls", permissions: new[] { GuildPermission.CreateInstantInvite })]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class ServerGroupCalls {
        private DatabaseSyncedList<GroupData> Groups { get; } = DatabaseSyncedList.Instance<GroupData>();

        public static ServerGroupCalls Instance { get; } = new();

        private DatabaseSyncedList<RunningCall> RunningCalls { get; } = DatabaseSyncedList.Instance<RunningCall>();

        static ServerGroupCalls() {
        }

        private ServerGroupCalls() {
            RunningCalls.ToAsyncEnumerable().ForEachAwaitAsync(async call => await UpdateInvites(call.Group, call.Channel, call.StartedBy, (await RolesUserIds(call.Channel.Guild, call.Group)).ToArray(), call)).Wait();
        }

        [Command("addgrouprole", "adds a role to the current call group", Accessibility.Admin)]
        public async Task AddRole([CommandParameter] SocketGuildUser user, [CommandParameter(0)] SocketRole addedRole,
            [CommandParameter] ISocketMessageChannel textChannel) {
            IVoiceChannel channel = user?.VoiceChannel;
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
            IVoiceChannel channel = user?.VoiceChannel;
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
            IVoiceChannel channel = user?.VoiceChannel;
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
                await UpdateInvites(group, channel, user, roleUserIds);
            }
        }

        public async Task CheckLeaving(SocketUser user, SocketVoiceChannel channel) {
            GroupData group = Groups.FirstOrDefault(g => g.ChannelId == channel.Id);
            if (group == null) {
                return;
            }

            ulong[] roleUserIds = (await RolesUserIds(channel.Guild, group)).ToArray();
            if (group.Auto && (group.UserIds.Contains(user.Id) || roleUserIds.Contains(user.Id))) {
                IEnumerable<IGuildUser> users = (await UsersInChannel(channel, group, roleUserIds)).ToArray();
                RunningCall call = RunningCalls[group.ChannelId];
                if (!users.Any()) {
                    call.Timeout = DateTime.UtcNow;
                    RunningCalls.Update(call);
                }
                await UpdateInvites(group, channel, user, roleUserIds, call, users);
            }
        }

        [Command("creategroup", "creates a call group in your current channel", Accessibility.Admin)]
        public async Task CreateGroup([CommandParameter] SocketGuildUser user, [CommandParameter(0, false)] bool auto,
            [CommandParameter] ISocketMessageChannel textChannel) {
            IVoiceChannel channel = user?.VoiceChannel;
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

        [Command("groupinfo", "gives information about a specific group", Accessibility.Admin)]
        public async Task GroupInfo([CommandParameter] SocketGuildUser user, [CommandParameter] ISocketMessageChannel textChannel, [CommandParameter(0)] int groupNumber) {
            SocketGuild guild = user?.Guild;
            GroupData[] groups = Groups.Where(g => Program.Client.GetVoiceChannelById(g.ChannelId)?.GuildId == guild?.Id && guild?.Id != null).ToArray();
            if (groups.Length <= groupNumber) {
                throw new KudosArgumentOutOfRangeException("groupNumber doesn't exist, try a lower number");
            }
            GroupData group = groups[groupNumber];

            string msg = group.RoleIds.Aggregate($"**{Program.Client.GetVoiceChannelById(group.ChannelId).Name}**\nAuto Invite: {(group.Auto ? "On" : "Off")}\nRoles:\n", (s, r) => $"{s}{guild.GetRole(r).Name}\n");
            msg += group.UserIds.Aggregate("Users:\n", (s, u) => $"{s}{guild.GetUser(u).Username}\n");
            await Messaging.Instance.SendMessage(textChannel, msg);
        }

        [Command("listgroups", "lists all groups on the current server", Accessibility.Admin)]
        public async Task ListGroups([CommandParameter] SocketGuildUser user, [CommandParameter] ISocketMessageChannel textChannel) {
            IEnumerable<GroupData> groups = Groups.Where(g => Program.Client.GetVoiceChannelById(g.ChannelId)?.GuildId == user?.Guild?.Id && user?.Guild?.Id != null);
            int index = 0;
            string list = groups.Aggregate("Groups:\n", (s, group) => s + $"{index++}: **{Program.Client.GetVoiceChannelById(group.ChannelId).Name}** {group.UserIds.Count} Users {group.RoleIds.Count} Roles");
            await Messaging.Instance.SendMessage(textChannel, list);
        }

        [Command("deletegroup", "deletes the call group in your current channel", Accessibility.Admin)]
        public async Task DeleteGroup([CommandParameter] SocketGuildUser user, [CommandParameter] ISocketMessageChannel textChannel) {
            IVoiceChannel channel = user?.VoiceChannel;
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

        private static async Task DeleteInvites(RunningCall call) {
            foreach (IUserMessage invite in call.CurrentInvites) {
                try {
                    await invite.ModifyAsync(message => {
                        message.Embed = new EmbedBuilder().SetDefaults()
                            .WithDescription($"**{(call.StartedBy.GetOrCreateDMChannelAsync().WaitForResult().Id == invite.Channel.Id ? "You" : call.StartedBy.Username)}** started a call in **{call.Channel.Name}** in {call.Channel.Guild.Name} that lasted for {(DateTime.Now - call.Start).Add(new TimeSpan(0, -5, 0)).Readable()}")
                            .Build();
                    });
                } catch (Exception e) {
                    LogService.Instance.Log($"A server group call invite could not be deleted (updated)\n{e}", LogService.LogType.Main, Google.Cloud.Logging.Type.LogSeverity.Notice);
                }
            }
        }

        [Command("invitegroup", "sends a dm to all group members to invite them to join your channel")]
        public async Task InviteGroup([CommandParameter] SocketGuildUser user, [CommandParameter] ISocketMessageChannel textChannel) {
            SocketVoiceChannel channel = user?.VoiceChannel;
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

        private static async Task RefreshInvites(IEnumerable<IUserMessage> currentInvites, IEnumerable<IGuildUser> users) {
            string inChannelString = users.Aggregate("\u200B", (current, guildUser) => current + $"{guildUser}\n");

            foreach (IUserMessage invite in currentInvites) {
                try {
                    await invite.ModifyAsync(message => {
                        message.Embed = new EmbedBuilder().SetDefaults()
                            .WithDescription(invite.Embeds.First().Description)
                            .AddField("Current Users", inChannelString)
                            .WithTimestamp(invite.Timestamp)
                            .Build();
                    });
                } catch (Exception e) {
                    LogService.Instance.Log($"A server group call invite could not be updated\n{e}", LogService.LogType.Main, Google.Cloud.Logging.Type.LogSeverity.Notice);
                }
            }
        }

        [Command("removegrouprole", "removes a role from the current call group", Accessibility.Admin)]
        public async Task RemoveRole([CommandParameter] SocketGuildUser user, [CommandParameter(0)] SocketRole removedRole,
            [CommandParameter] ISocketMessageChannel textChannel) {
            IVoiceChannel channel = user?.VoiceChannel;
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
            IVoiceChannel channel = user?.VoiceChannel;
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

        private async Task SendInvites(GroupData group, SocketVoiceChannel channel, IUser user, ulong[] roleUserIds, RunningCall call = null, IEnumerable<IGuildUser> users = null) {
            call ??= RunningCalls[group.ChannelId];

            if (call != null) {
                if (call.Timeout > DateTime.UtcNow.AddMinutes(-5)) {
                    throw new KudosInvalidOperationException($"There is still a Timeout for {(call.Timeout - DateTime.UtcNow.AddMinutes(-5)).LikeInput()}");
                }
            } else {
                RunningCalls.Add(new RunningCall(user, group, channel) { CurrentInvites = new List<IUserMessage>() });
            }
            int errorCount = 0;
            HashSet<ulong> userIds = new();
            userIds.UnionWith(group.UserIds);
            userIds.UnionWith(roleUserIds);
            IEnumerable<IGuildUser> alreadyInChannel = (users ?? await UsersInChannel(channel, group, roleUserIds)).ToArray();
            IReadOnlyCollection<IInviteMetadata> invites = await channel.GetInvitesAsync();

            IInviteMetadata invite =
                invites.FirstOrDefault(i => i.ChannelId == channel.Id && !i.IsTemporary && i.Inviter.Id == Program.Client.BotUserId)
                ?? await channel.CreateInviteAsync();

            string inChannelString = alreadyInChannel.Aggregate("", (current, guildUser) => current + $"{guildUser}\n");
            foreach (ulong groupUserId in userIds) {
                try {
                    SocketUser groupUser = Program.Client.GetSocketUserById(groupUserId);
                    if (groupUser.IsBot) {
                        continue;
                    }
                    IDMChannel groupUserChannel = await groupUser.GetOrCreateDMChannelAsync();

                    IUserMessage message = await Messaging.Instance.SendEmbed(groupUserChannel,
                        new EmbedBuilder().SetDefaults()
                            .WithDescription(
                                $"Hey, **{(user.GetOrCreateDMChannelAsync().WaitForResult().Id == invite.Channel.Id ? "You" : user.Username)}** invited you to join the voice call [**{channel.Name}** in {channel.Guild.Name}]({invite.Url})")
                            .AddField("Current Users", inChannelString));
                    if (group.Auto) {
                        call.CurrentInvites.Add(message);
                    }
                } catch (Exception) {
                    errorCount++;
                }
            }
            if (errorCount > 0) {
                throw new KudosUnauthorizedException($"{errorCount} users could not be notified");
            }

            call.Timeout = DateTime.UtcNow;
            RunningCalls.Update(call);
        }

        private async Task UpdateInvites(GroupData group, SocketVoiceChannel channel, IUser user, ulong[] roleUserIds, RunningCall call = null, IEnumerable<IGuildUser> users = null) {
            users = (users ?? await UsersInChannel(channel, group, roleUserIds)).ToArray();
            call ??= RunningCalls[group.ChannelId];
            if (call != null) {
                await RefreshInvites(call.CurrentInvites, users);
                if (users.Any()) {
                    return;
                }

                new Func<Task>(async () => {
                    await Task.Delay(new TimeSpan(0, 5, 0));
                    if (call.Timeout + new TimeSpan(0, 5, 0) > DateTime.UtcNow || (await UsersInChannel(channel, group, roleUserIds)).Any()) {
                        return;
                    }
                    await DeleteInvites(call);
                    RunningCalls.Remove(call);
                }).RunAsyncSave();
                return;
            }
            if (users.Any()) {
                await SendInvites(group, channel, user, roleUserIds, call, users);
            }
        }

        private static async Task<IEnumerable<IGuildUser>> UsersInChannel(IGuildChannel channel, GroupData group, IEnumerable<ulong> roleUserIds) {
            return (await channel.GetUsersAsync().AwaitAll()).Where(channelUser =>
                group.UserIds.Contains(channelUser.Id) || roleUserIds.Contains(channelUser.Id));
        }
    }
}