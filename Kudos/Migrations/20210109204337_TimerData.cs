using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Kudos.Migrations
{
    public partial class TimerData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TimerData",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    ChannelId = table.Column<ulong>(nullable: false),
                    End = table.Column<DateTime>(nullable: false),
                    Message = table.Column<string>(nullable: true),
                    Repeat = table.Column<TimeSpan>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimerData", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TimerData");
        }
    }
}
