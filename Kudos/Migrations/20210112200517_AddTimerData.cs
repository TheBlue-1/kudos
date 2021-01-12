#region
using System;
using Microsoft.EntityFrameworkCore.Migrations;

// ReSharper disable UnusedMember.Global
#endregion

namespace Kudos.Migrations {
	public partial class AddTimerData : Migration {
		protected override void Down(MigrationBuilder migrationBuilder) {
			migrationBuilder.DropTable("TimerData");
		}

		protected override void Up(MigrationBuilder migrationBuilder) {
			migrationBuilder.CreateTable("TimerData",
				table => new {
					Id = table.Column<string>(nullable: false),
					OwnerId = table.Column<ulong>(nullable: false),
					ChannelId = table.Column<ulong>(nullable: false),
					End = table.Column<DateTime>(nullable: false),
					Message = table.Column<string>(nullable: true),
					Repeat = table.Column<TimeSpan>(nullable: false)
				}, constraints: table => { table.PrimaryKey("PK_TimerData", x => x.Id); });
		}
	}
}
