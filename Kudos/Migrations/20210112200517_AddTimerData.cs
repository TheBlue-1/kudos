#region

using Microsoft.EntityFrameworkCore.Migrations;
using System;

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
                    Id = table.Column<ulong>(nullable: false),
                    ChannelId = table.Column<ulong>(nullable: false),
                    End = table.Column<DateTime>(nullable: false),
                    Message = table.Column<string>(nullable: true),
                    OwnerId = table.Column<ulong>(nullable: false),
                    Repeat = table.Column<TimeSpan>(nullable: false)
                }, constraints: table => { table.PrimaryKey("PK_TimerData", x => x.Id); });
        }
    }
}