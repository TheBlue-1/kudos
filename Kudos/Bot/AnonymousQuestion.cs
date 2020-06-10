#region
using System;
using Discord;
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

		public async void Answer(ulong questionId, string message, SocketUser answerer) {
			if (!AnonymousQuestions.ContainsKey(questionId)) {
				return;
			}

			QuestionData question = AnonymousQuestions[questionId];
			if (answerer.Id != question.Receiver) {
				return;
			}

			try {
				IDMChannel receiverChannel = await answerer.GetOrCreateDMChannelAsync();
				SocketUser questionnaire = Program.Client.GetUserById(question.Questionnaire);
				IDMChannel questionnaireChannel = await questionnaire.GetOrCreateDMChannelAsync();

				await questionnaireChannel.SendMessageAsync(
					$"Your Question to {answerer.Mention}: ```{question.Question}``` has been answered. The answer is: ```{message}```");
				await receiverChannel.SendMessageAsync("answer submitted successfully");
				AnonymousQuestions.Remove(questionId);
			}
			catch {
				// ignored (user has disabled direct messages from guild members)
			}
		}

		public async void AskAnonymous(string message, SocketUser receiver, SocketUser questionnaire) {
			ulong id = NextId;
			AnonymousQuestions[id] = new QuestionData { Question = message, Questionnaire = questionnaire.Id, Receiver = receiver.Id };
			try {
				IDMChannel receiverChannel = await receiver.GetOrCreateDMChannelAsync();
				IDMChannel questionnaireChannel = await questionnaire.GetOrCreateDMChannelAsync();

				await receiverChannel.SendMessageAsync(
					$"Hello, someone has a question for you, but wants to stay anonymous. Here it is: ```{message}``` To answer the question please write `{MessageInterpreter.Prefix}answer {id} [answer]`.");
				await questionnaireChannel.SendMessageAsync("Question sent successfully. Answer will be sent to you.");
			}
			catch {
				// ignored (user has disabled direct messages from guild members)
			}
		}
	}
}
