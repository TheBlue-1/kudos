#region
using System;
#endregion

namespace Kudos.Models {
	[Serializable]
	public class QuestionData {
		public string Question { get; set; }
		public ulong Questionnaire { get; set; }
		public ulong Receiver { get; set; }
	}
}
