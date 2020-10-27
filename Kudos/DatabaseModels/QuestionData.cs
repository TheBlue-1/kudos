#region
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#endregion

namespace Kudos.DatabaseModels {
	public class QuestionData {
		public ulong Answerer { get; set; }
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public ulong Id { get; set; }
		public string Question { get; set; }
		public ulong Questionnaire { get; set; }
	}
}
