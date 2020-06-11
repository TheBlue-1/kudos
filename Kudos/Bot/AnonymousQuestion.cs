#region
using System;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Kudos.Models;
using Kudos.Utils;
#endregion

namespace Kudos.Bot {
	public class AnonymousQuestion {
		private AsyncFileSyncedDictionary<ulong, QuestionData> AnonymousQuestions { get; } =
			new AsyncFileSyncedDictionary<ulong, QuestionData>("anonymousQuestions");

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

		public async void Answer(ulong questionId, string message, SocketUser answerer, ISocketMessageChannel channel) {
			if (!AnonymousQuestions.ContainsKey(questionId)) {
				return;
			}

			QuestionData question = AnonymousQuestions[questionId];
			if (answerer.Id != question.Answerer) {
				return;
			}

			RestUser restAnswerer = await Program.Client.GetRestUserById(answerer.Id);
			RestUser restQuestionnaire = await Program.Client.GetRestUserById(question.Questionnaire);

			IDMChannel answererChannel = await restAnswerer.GetOrCreateDMChannelAsync();
			IDMChannel questionnaireChannel = await restQuestionnaire.GetOrCreateDMChannelAsync();
			try {
				await questionnaireChannel.SendMessageAsync(
					$"Your Question to {answerer.Mention}: ```{question.Question}``` has been answered. The answer is: ```{message}```");
				await answererChannel.SendMessageAsync("answer submitted successfully");
			}
			catch (Exception) {
				await channel.SendMessageAsync(
					"You or the questionnaire has disabled private messages from this server, so I won't be able to send your answer or the success message");
			}
			AnonymousQuestions.Remove(questionId);
		}

		public async void AskAnonymous(string message, SocketUser answerer, SocketUser questionnaire, ISocketMessageChannel channel) {
			if (answerer == null) {
				return;
			}
			ulong id = NextId;
			AnonymousQuestions[id] = new QuestionData { Question = message, Questionnaire = questionnaire.Id, Answerer = answerer.Id };

			RestUser restAnswerer = await Program.Client.GetRestUserById(answerer.Id);
			RestUser restQuestionnaire = await Program.Client.GetRestUserById(questionnaire.Id);

			IDMChannel answererChannel = await restAnswerer.GetOrCreateDMChannelAsync();
			IDMChannel questionnaireChannel = await restQuestionnaire.GetOrCreateDMChannelAsync();
			try {
				await answererChannel.SendMessageAsync(
					$"Hello, someone has a question for you, but wants to stay anonymous. Here it is: ```{message}``` To answer the question please write `{MessageInterpreter.Prefix}answer {id} [answer]`.");
				await questionnaireChannel.SendMessageAsync("Question sent successfully. Answer will be sent to you.");
			}
			catch (Exception) {
				await channel.SendMessageAsync(
					$"You or {answerer.Mention} has disabled private messages from this server, so I won't be able to send the question or the answer");
			}
		}
	}
}
