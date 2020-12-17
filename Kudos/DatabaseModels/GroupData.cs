#region
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
// ReSharper disable UnusedMember.Global
#endregion

namespace Kudos.DatabaseModels {
	public class GroupData {
		private List<ulong> _roleIds;
		private List<ulong> _userIds;
		public bool Auto { get; set; }

		[Key]
		public ulong ChannelId { get; set; }
		public List<ulong> RoleIds {
			get => _roleIds ??= new List<ulong>();
			set => _roleIds = value;
		}
		public List<ulong> UserIds {
			get => _userIds ??= new List<ulong>();
			set => _userIds = value;
		}
	}
}
