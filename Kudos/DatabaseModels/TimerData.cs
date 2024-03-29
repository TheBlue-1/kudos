﻿#region

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#endregion

namespace Kudos.DatabaseModels {

    public class TimerData {
        public ulong ChannelId { get; set; }
        public DateTime End { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong Id { get; set; }

        public string Message { get; set; }
        public ulong OwnerId { get; set; }
        public TimeSpan Repeat { get; set; }
    }
}