#region

using Kudos.Utils;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

#endregion

namespace Kudos.DatabaseModels {

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "PartialTypeWithSinglePart")]
    public partial class KudosDataContext : DbContext {
        public DbSet<BanData> BanData { get; set; }
        public DbSet<GroupData> GroupData { get; set; }
        public DbSet<HonorData> HonorData { get; set; }
        public DbSet<QuestionData> QuestionData { get; set; }
        public DbSet<TimerData> TimerData { get; set; }

        public KudosDataContext() {
        }

        public KudosDataContext(DbContextOptions<KudosDataContext> options) : base(options) {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            if (!optionsBuilder.IsConfigured) {
                optionsBuilder.UseSqlite("DataSource=" + Path.Combine(FileService.Instance.ApplicationFolderPath, "KudosData.db"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            // ReSharper disable once InvocationIsSkipped
            OnModelCreatingPartial(modelBuilder);
            modelBuilder.Entity<GroupData>()
                .Property(group => group.UserIds)
                .HasConversion(v => string.Join(';', v), v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(ulong.Parse).ToList());
            modelBuilder.Entity<GroupData>()
                .Property(group => group.RoleIds)
                .HasConversion(v => string.Join(';', v), v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(ulong.Parse).ToList());
        }

        // ReSharper disable once PartialMethodWithSinglePart
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}