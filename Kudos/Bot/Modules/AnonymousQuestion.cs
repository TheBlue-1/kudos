#region
using System;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Kudos.Attributes;
using Kudos.Exceptions;
using Kudos.Extensions;
using Kudos.Models;
using Kudos.Utils;

// ReSharper disable UnusedMember.Global
#endregion

namespace Kudos.Bot.Modules {
	[CommandModule("AnonymousQuestions")]
	public class AnonymousQuestion {
		private AsyncThreadsafeFileSyncedDictionary<ulong, QuestionData> AnonymousQuestions { get; } =
			new AsyncThreadsafeFileSyncedDictionary<ulong, QuestionData>("anonymousQuestions");

		public static AnonymousQuestion Instance { get; } = new AnonymousQuestion();

		private ulong NextId {
			get {
				for (ulong i = 0; i < ulong.MaxValue; i++) {
					if (!AnonymousQuestions.ContainsKey(i)) {
						return i;
					}
				}

				throw new Exception("ulong id limit exceeded");
			}
		}

		static AnonymousQuestion() { }

		private AnonymousQuestion() { }

		[Command("answer", "answers anonymous question")]
		public async Task Answer([CommandParameter(0)] ulong questionId, [CommandParameter(1)] string message, [CommandParameter] SocketUser answerer) {
			if (!AnonymousQuestions.ContainsKey(questionId)) {
				throw new KudosKeyNotFoundException($"No question found for id {questionId}");
			}

			QuestionData question = AnonymousQuestions[questionId];
			if (answerer.Id != question.Answerer) {
				throw new KudosUnauthorizedException("The question with this id isn't meant for you", "User with wrong id tried to answer the question");
			}

			RestUser restQuestionnaire = await Program.Client.GetRestUserById(question.Questionnaire);

			IDMChannel answererChannel = await answerer.DmChannel();
			IDMChannel questionnaireChannel = await restQuestionnaire.GetOrCreateDMChannelAsync();
			try {
				await Messaging.Instance.SendMessage(questionnaireChannel,
					$"Your Question to {answerer.Mention}: ```{question.Question}``` has been answered. The answer is: ```{message}```");
				await Messaging.Instance.SendMessage(answererChannel, "answer submitted successfully");
			}
			catch (Exception) {
				throw new KudosUnauthorizedException("You or the questionnaire has disabled private messages from this server", "user has dms disabled");
			}
			AnonymousQuestions.Remove(questionId);
		}

		[Command("ask", "asks anonymous question")]
		public async Task AskAnonymous([CommandParameter(1)] string message, [CommandParameter(0)] SocketUser answerer,
			[CommandParameter] SocketUser questionnaire) {
			ulong id = NextId;
			AnonymousQuestions[id] = new QuestionData { Question = message, Questionnaire = questionnaire.Id, Answerer = answerer.Id };

			RestUser restAnswerer = await Program.Client.GetRestUserById(answerer.Id);
			RestUser restQuestionnaire = await Program.Client.GetRestUserById(questionnaire.Id);

			IDMChannel answererChannel = await restAnswerer.GetOrCreateDMChannelAsync();
			IDMChannel questionnaireChannel = await restQuestionnaire.GetOrCreateDMChannelAsync();
			try {
				await Messaging.Instance.SendMessage(answererChannel,
					$"Hello, someone has a question for you, but wants to stay anonymous. Here it is: ```{message}``` To answer the question please write `{MessageInterpreter.Prefix}answer {id} [answer]`.");
				await Messaging.Instance.SendMessage(questionnaireChannel, "Question sent successfully. Answer will be sent to you.");
			}
			catch (Exception) {
				throw new KudosUnauthorizedException($"You or {answerer.Mention} has disabled private messages from this server", "user has dms disabled");
			}
		}
	}
}
