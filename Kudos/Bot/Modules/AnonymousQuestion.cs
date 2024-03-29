﻿#region

using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Kudos.Attributes;
using Kudos.DatabaseModels;
using Kudos.Exceptions;
using Kudos.Models;
using Kudos.Utils;
using System;
using System.Linq;
using System.Threading.Tasks;

// ReSharper disable UnusedMember.Global
#endregion

namespace Kudos.Bot.Modules {

    [CommandModule("AnonymousQuestions")]
    public class AnonymousQuestion {
        private DatabaseSyncedList<QuestionData> AnonymousQuestions { get; } = DatabaseSyncedList.Instance<QuestionData>();

        public static AnonymousQuestion Instance { get; } = new();

        private ulong NextId {
            get {
                for (ulong i = 0; i < ulong.MaxValue; i++) {
                    if (AnonymousQuestions.All(questionData => questionData.Id != i)) {
                        return i;
                    }
                }

                throw new Exception("ulong id limit exceeded");
            }
        }

        static AnonymousQuestion() {
        }

        private AnonymousQuestion() {
        }

        [Command("answer", "answers anonymous question")]
        public async Task Answer([CommandParameter(0)] ulong questionId, [CommandParameter(1)] string message, [CommandParameter] SocketUser answerer) {
            if (answerer.IsBot) {
                throw new KudosInvalidOperationException("You can't send questions to a bot");
            }
            QuestionData question = AnonymousQuestions.FirstOrDefault(questionData => questionData.Id == questionId)
                ?? throw new KudosKeyNotFoundException($"No question found for id {questionId}");

            if (answerer.Id != question.Answerer) {
                throw new KudosUnauthorizedException("The question with this id isn't meant for you", "User with wrong id tried to answer the question");
            }

            RestUser restQuestionnaire = await Program.Client.GetRestUserById(question.Questionnaire);

            IDMChannel answererChannel = await answerer.GetOrCreateDMChannelAsync();
            IDMChannel questionnaireChannel = await restQuestionnaire.GetOrCreateDMChannelAsync();
            try {
                await Messaging.Instance.SendMessage(questionnaireChannel,
                    $"Your Question to {answerer.Mention}: ```{question.Question}``` has been answered. The answer is: ```{message}```");
                await Messaging.Instance.SendMessage(answererChannel, "answer submitted successfully");
            } catch (Exception) {
                throw new KudosUnauthorizedException("You or the questionnaire has disabled private messages from this server", "user has dms disabled");
            }
            AnonymousQuestions.Remove(question);
        }

        [Command("ask", "asks anonymous question")]
        public async Task AskAnonymous([CommandParameter(1)] string message, [CommandParameter(0)] SocketUser answerer,
            [CommandParameter] SocketUser questionnaire) {
            if (AnonymousQuestions.Count(q => q.Questionnaire == questionnaire.Id) >= 20) {
                throw new KudosInvalidOperationException(
                    "You have reached the current limit of 20 open anonymous questions. If you have a valid reason to use more than that please get in touch with our support team on our Support server.");
            }
            ulong id = NextId;
            AnonymousQuestions.Add(new QuestionData { Id = id, Question = message, Questionnaire = questionnaire.Id, Answerer = answerer.Id });

            RestUser restAnswerer = await Program.Client.GetRestUserById(answerer.Id);
            RestUser restQuestionnaire = await Program.Client.GetRestUserById(questionnaire.Id);

            IDMChannel answererChannel = await restAnswerer.GetOrCreateDMChannelAsync();
            IDMChannel questionnaireChannel = await restQuestionnaire.GetOrCreateDMChannelAsync();
            try {
                await Messaging.Instance.SendMessage(answererChannel,
                    $"Hello, someone has a question for you, but wants to stay anonymous. Here it is: ```{message}``` To answer the question please write `{SettingsManager.Instance.SettingsFor(answerer.Id)[SettingNames.Prefix].StringValue}answer {id} [answer]`.");
                await Messaging.Instance.SendMessage(questionnaireChannel, "Question sent successfully. Answer will be sent to you.");
            } catch (Exception) {
                throw new KudosUnauthorizedException($"You or {answerer.Mention} has disabled private messages from this server", "user has dms disabled");
            }
        }
    }
}