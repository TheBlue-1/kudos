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
	[CommandModule("Honor")]
	public sealed class Honor {
		private const byte MaxHonorPerDay = 7;
		public static readonly IEmote[] HonorEmojis = {
			new Emoji("1️⃣"), new Emoji("2️⃣"), new Emoji("3️⃣"), new Emoji("4️⃣"), new Emoji("5️⃣"), new Emoji("6️⃣"), new Emoji("7️⃣")
		};
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

		private DatabaseSyncedList<HonorData> HonorData { get; } = new DatabaseSyncedList<HonorData>();
		public static Honor Instance { get; } = new Honor();

		static Honor() { }

		private Honor() { }

		private int BalanceOf(ulong userId) {
			return HonorData.Where(honorData => honorData.Honored == userId).Sum(honorData => honorData.Honor);
		}

		[Command("dishonor", "removes honor points for user")]
		public async Task DishonorUser([CommandParameter(0)] SocketUser honoredUser, [CommandParameter] SocketUser honoringUser,
			[CommandParameter(1, 1)] int count, [CommandParameter] ISocketMessageChannel channel) {
			count = HonorCount(honoredUser, honoringUser, count);

			HonorData.Add(new HonorData { Honor = -count, Honored = honoredUser.Id, Honorer = honoringUser.Id, Timestamp = DateTime.Now });

			await Messaging.Instance.SendMessage(channel, $"You successfully removed ***{count}*** honor points for ***{honoredUser.Mention}***!");
		}

		public EmbedBuilder GuildStatsEmbed(IEnumerable<SocketUser> users, TimeSpan time) {
			IEnumerable<HonorData> filteredHonorData = HonorData;
			if (time > new TimeSpan(0)) {
				filteredHonorData = filteredHonorData.Where(x => x.Timestamp > DateTime.Now - time);
			}
			users ??= filteredHonorData.Select(honorData => honorData.Honored).Distinct().Select(id => Program.Client.GetSocketUserById(id));

			IEnumerable<SocketUser> socketUsers = users as SocketUser[] ?? users.ToArray();
			IEnumerable<ulong> ids = socketUsers.Select(socketUser => socketUser.Id);

			var balances = filteredHonorData.GroupBy(honorData => honorData.Honored)
				.Where(honorData => ids.Contains(honorData.Key))
				.Select(honorData => new { Value = honorData.Sum(y => y.Honor), User = socketUsers.First(socketUser => socketUser.Id == honorData.Key) })
				.OrderByDescending(pair => pair.Value);

			EmbedBuilder embed = new EmbedBuilder().SetDefaults().WithTitle("🌟Leader board🌟");
			string text = "";
			int counter = 1;
			foreach (var balance in balances) {
				text += $"{counter}. *{balance.User}* with an honor of **{balance.Value}** \n";
				if (counter == 20) {
					break;
				}
				counter++;
			}
			embed.WithDescription(text);
			return embed;
		}

		public int HonorCount(IUser honoredUser, IUser honoringUser, int count) {
			if (honoringUser.Id == honoredUser.Id) {
				throw new KudosUnauthorizedException("You are not allowed to honor yourself");
			}

			int usedHonor = UsedHonorOf(honoringUser.Id);

			count = count >= MaxHonorPerDay - usedHonor ? MaxHonorPerDay - usedHonor : count <= 0 ? 1 : count;
			if (count == 0) {
				throw new KudosUnauthorizedException("You already used all your daily honor points");
			}
			return count;
		}

		[Command("honor", "adds honor points for user")]
		public async Task HonorUser([CommandParameter(0)] IUser honoredUser, [CommandParameter] IUser honoringUser, [CommandParameter(1, 1)] int count,
			[CommandParameter] IMessageChannel channel) {
			count = HonorCount(honoredUser, honoringUser, count);

			HonorData.Add(new HonorData { Honor = count, Honored = honoredUser.Id, Honorer = honoringUser.Id, Timestamp = DateTime.Now });

			await Messaging.Instance.SendMessage(channel, $"You honored ***{honoredUser.Mention}*** with ***{count}*** Points!");
		}

		public async Task HonorUserWithReaction(IUserMessage message, SocketReaction reaction) {
			if (message.Embeds.FirstOrDefault() == null || !message.Embeds.First().Description.StartsWith("Hey, do you want to honor ")) {
				return;
			}
			int honor = HonorEmojis.ToList().IndexOf(reaction.Emote) + 1;
			if (honor == 0) {
				return;
			}
			IUser honorer = reaction.User.Value;
			string description = message.Embeds.First().Description;
			SocketUser honored = description.Substring(26, description.Length - 102).ToValue<SocketUser>(0);
			await HonorUser(honored, honorer, honor, message.Channel);
		}

		[Command("leaders", "shows the most highly honored people of the server")]
		public async Task SendGuildStats([CommandParameter] ISocketMessageChannel channel, [CommandParameter(0, 0)] TimeSpan time) {
			if (!(channel is SocketGuildChannel guildChannel)) {
				throw new KudosUnauthorizedException("this command can only be used servers");
			}

			// ReSharper disable once CoVariantArrayConversion
			IEnumerable<SocketUser> users = guildChannel.Guild.Users;
			await Messaging.Instance.SendEmbed(channel, GuildStatsEmbed(users, time));
		}

		[Command("balance", "shows the honor point balance")]
		public async Task SendHonorBalance([CommandParameter(0, ParameterType.SpecialDefaults.IndexLess)]
			SocketUser user, [CommandParameter] ISocketMessageChannel channel) {
			int honor = BalanceOf(user.Id);
			string honorMessage;

			if (honor < -100) {
				int index = Program.Random.Next(0, HonorFeedbackLowest.Length);
				honorMessage = HonorFeedbackLowest[index];
			} else if (honor < -50) {
				int index = Program.Random.Next(0, HonorFeedbackLower.Length);
				honorMessage = HonorFeedbackLower[index];
			} else if (honor < 0) {
				int index = Program.Random.Next(0, HonorFeedbackLow.Length);
				honorMessage = HonorFeedbackLow[index];
			} else if (honor < 50) {
				int index = Program.Random.Next(0, HonorFeedbackHigh.Length);
				honorMessage = HonorFeedbackHigh[index];
			} else if (honor <= 100) {
				int index = Program.Random.Next(0, HonorFeedbackHigher.Length);
				honorMessage = HonorFeedbackHigher[index];
			} else {
				int index = Program.Random.Next(0, HonorFeedbackHighest.Length);
				honorMessage = HonorFeedbackHighest[index];
			}

			await Messaging.Instance.SendMessage(channel, $"***{user.Mention}*** has ***{honor}*** honor points. \n***{honorMessage}***");
		}

		private int UsedHonorOf(ulong userId) {
			return HonorData.Where(honorData => honorData.Honorer == userId && honorData.Timestamp > DateTime.Now.AddHours(-24))
				.Sum(honorData => honorData.Honor);
		}
	}
}
