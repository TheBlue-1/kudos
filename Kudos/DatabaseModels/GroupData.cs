#region
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
#endregion

namespace Kudos.DatabaseModels {
	public class GroupData {
		public bool Auto { get; set; }

		[Key]
		public ulong ChannelId { get; set; }
		public List<ulong> UserIds { get; set; } = new List<ulong>();
	}
}
