#region
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
#endregion

namespace Kudos.DatabaseModels {
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	[SuppressMessage("ReSharper", "PartialTypeWithSinglePart")]
	public partial class KudosDataContext : DbContext {
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
		}

		// ReSharper disable once PartialMethodWithSinglePart
		partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
	}
}
