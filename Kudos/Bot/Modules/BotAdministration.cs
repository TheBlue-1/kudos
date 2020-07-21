#region
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using DiscordBotsList.Api.Objects;
using Kudos.Attributes;
using Kudos.Exceptions;

// ReSharper disable UnusedMember.Global
#endregion

namespace Kudos.Bot.Modules {
	[CommandModule("BotAdministration", true)]
	public sealed class BotAdministration {
		public static BotAdministration Instance { get; } = new BotAdministration();

		static BotAdministration() { }

		private BotAdministration() { }

		[Command("guilds", "shows all guilds of the server")]
		public async Task SendGuilds([CommandParameter] ISocketMessageChannel channel) {
			IReadOnlyCollection<SocketGuild> guilds = Program.Client.Guilds;
			string message = $"I am present on {guilds.Count} guilds";
			if (!Program.IsBotListBot) {
				message += "\nI am not the real Kudos (just a test/beta version)";
			}
			message = guilds.Aggregate(message, (current, guild) => current + $"\n{guild.Name}");

			await Messaging.Instance.SendMessage(channel, message);
		}

		[Command("votes", "shows who voted for the server")]
		public async Task SendVotes([CommandParameter] ISocketMessageChannel channel) {
			if (Program.BotList == null) {
				throw new KudosUnauthorizedException("bot is not allowed to see this information");
			}
			List<IDblEntity> voters = await Program.BotList.ThisBot.GetVotersAsync();
			var countedVotes = voters.GroupBy(voter => voter.Id).Select(voter => new { voter.First().Id, Count = voter.Count(), voter.First().Username });

			var countedVotesList = countedVotes.ToList();
			string message =
				$"Total Votes: {Program.BotList.ThisBot.Points}\nThis Month Votes: coming soon\nVoters Count: {countedVotesList.Count}\nTheir total votes: {voters.Count}";
			message = countedVotesList.Aggregate(message, (current, voter) => current + $"\n{voter.Username} ({voter.Count})");
			await Messaging.Instance.SendMessage(channel, message);
		}
	}
}
