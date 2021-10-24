#region

using System.ComponentModel.DataAnnotations;

#endregion

namespace Kudos.DatabaseModels {

    public class BanData {
        public bool HardBan { get; set; }

        [Key]
        public ulong UserId { get; set; }
    }
}