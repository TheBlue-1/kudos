#region
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.EntityFrameworkCore;
#endregion

namespace Kudos.DatabaseModels {
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	[SuppressMessage("ReSharper", "PartialTypeWithSinglePart")]
	public partial class KudosDataContext : DbContext {
		public DbSet<GroupData> GroupData { get; set; }
		public DbSet<HonorData> HonorData { get; set; }
		public DbSet<QuestionData> QuestionData { get; set; }

		public KudosDataContext() { }

		public KudosDataContext(DbContextOptions<KudosDataContext> options) : base(options) { }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
			if (!optionsBuilder.IsConfigured) {
				optionsBuilder.UseSqlite("DataSource=KudosData.db");
			}
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder) {
			// ReSharper disable once InvocationIsSkipped
			OnModelCreatingPartial(modelBuilder);
			modelBuilder.Entity<GroupData>()
				.Property(group => group.UserIds)
				.HasConversion(v => string.Join(';', v), v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(ulong.Parse).ToList());
		}

		// ReSharper disable once PartialMethodWithSinglePart
		partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
	}
}
