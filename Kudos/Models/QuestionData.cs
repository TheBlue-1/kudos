﻿#region
using System;
#endregion

namespace Kudos.Models {
	[Serializable]
	public class QuestionData {
		public ulong Answerer { get; set; }
		public string Question { get; set; }
		public ulong Questionnaire { get; set; }
	}
}