#region
using System;
using Kudos.DatabaseModels;
using Kudos.Utils;
using QuestionData = Kudos.Models.QuestionData;
#endregion

namespace Kudos {
	internal class Program {


		private static void Main() {
			Console.WriteLine("started");
			AsyncThreadsafeFileSyncedDictionary<ulong, QuestionData> anonymousQuestions =
				new AsyncThreadsafeFileSyncedDictionary<ulong, QuestionData>("anonymousQuestions");
			AsyncThreadsafeFileSyncedDictionary<ulong, int> balancesPerId = new AsyncThreadsafeFileSyncedDictionary<ulong, int>("balances");


			// ReSharper disable once CollectionNeverQueried.Local
			DatabaseSyncedList<DatabaseModels.QuestionData> newAnonymousQuestions =new DatabaseSyncedList<DatabaseModels.QuestionData>();
			// ReSharper disable once CollectionNeverQueried.Local
			DatabaseSyncedList<HonorData> newBalance=new DatabaseSyncedList<HonorData>();

			foreach ((ulong id, QuestionData question) in anonymousQuestions.Immutable) {
				newAnonymousQuestions.Add(new DatabaseModels.QuestionData {Answerer = question.Answerer,Id = id,Question = question.Question,Questionnaire = question.Questionnaire});
			}
			foreach ((ulong id, int balance) in balancesPerId.Immutable) {
				newBalance.Add(new HonorData {Honor = balance,Honored = id,Honorer = 719571683517792286 ,Timestamp = DateTime.Now});
			}
			Console.WriteLine("finished");

		}
	}
}
