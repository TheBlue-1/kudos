using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Kudos.DatabaseModels
{
   public class GroupData
    {
        public bool Auto { get; set; }
        public List<ulong> UserIds { get; set; }

		[Key]
        public ulong ChannelId { get; set; }
    }
}
