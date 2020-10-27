#region
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#endregion

namespace Kudos.DatabaseModels {
	public class HonorData {
		public int Honor { get; set; }
		public ulong Honored { get; set; }
		public ulong Honorer { get; set; }
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		// ReSharper disable once UnusedMember.Global
		public ulong Id { get; set; }
		public DateTime Timestamp { get; set; }
	}
}
