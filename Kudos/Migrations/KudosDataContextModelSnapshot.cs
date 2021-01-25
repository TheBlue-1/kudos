#region
using System;
using Kudos.DatabaseModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
#endregion

namespace Kudos.Migrations {
	[DbContext(typeof (KudosDataContext))]
	// ReSharper disable once UnusedMember.Global
	internal class KudosDataContextModelSnapshot : ModelSnapshot {
		protected override void BuildModel(ModelBuilder modelBuilder) {
		#pragma warning disable 612, 618
			modelBuilder.HasAnnotation("ProductVersion", "3.1.9");

			modelBuilder.Entity("Kudos.DatabaseModels.BanData", b => {
				b.Property<ulong>("UserId").ValueGeneratedOnAdd().HasColumnType("INTEGER");

				b.Property<bool>("HardBan").HasColumnType("INTEGER");

				b.HasKey("UserId");

				b.ToTable("BanData");
			});

			modelBuilder.Entity("Kudos.DatabaseModels.GroupData", b => {
				b.Property<ulong>("ChannelId").ValueGeneratedOnAdd().HasColumnType("INTEGER");

				b.Property<bool>("Auto").HasColumnType("INTEGER");

				b.Property<string>("RoleIds").HasColumnType("TEXT");

				b.Property<string>("UserIds").HasColumnType("TEXT");

				b.HasKey("ChannelId");

				b.ToTable("GroupData");
			});

			modelBuilder.Entity("Kudos.DatabaseModels.HonorData", b => {
				b.Property<ulong>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");

				b.Property<int>("Honor").HasColumnType("INTEGER");

				b.Property<ulong>("Honored").HasColumnType("INTEGER");

				b.Property<ulong>("Honorer").HasColumnType("INTEGER");

				b.Property<DateTime>("Timestamp").HasColumnType("TEXT");

				b.HasKey("Id");

				b.ToTable("HonorData");
			});

			modelBuilder.Entity("Kudos.DatabaseModels.QuestionData", b => {
				b.Property<ulong>("Id").HasColumnType("INTEGER");

				b.Property<ulong>("Answerer").HasColumnType("INTEGER");

				b.Property<string>("Question").HasColumnType("TEXT");

				b.Property<ulong>("Questionnaire").HasColumnType("INTEGER");

				b.HasKey("Id");

				b.ToTable("QuestionData");
			});

			modelBuilder.Entity("Kudos.DatabaseModels.TimerData", b => {
				b.Property<string>("Id").ValueGeneratedOnAdd().HasColumnType("TEXT");

				b.Property<ulong>("ChannelId").HasColumnType("INTEGER");

				b.Property<DateTime>("End").HasColumnType("TEXT");

				b.Property<string>("Message").HasColumnType("TEXT");

				b.Property<ulong>("OwnerId").HasColumnType("INTEGER");

				b.Property<TimeSpan>("Repeat").HasColumnType("TEXT");

				b.HasKey("Id");

				b.ToTable("TimerData");
			});
		#pragma warning restore 612, 618
		}
	}
}
