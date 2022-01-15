using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Kudos.Migrations {

    public partial class AddRunningCalls : Migration {

        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.AlterColumn<ulong>(
                name: "Id",
                table: "TimerData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.CreateTable(
                name: "RunningCall",
                columns: table => new {
                    ChannelId = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CurrentInvitesIds = table.Column<string>(type: "TEXT", nullable: true),
                    Timeout = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Start = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StartedById = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_RunningCall", x => x.ChannelId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropTable(
                name: "RunningCall");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "TimerData",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "INTEGER");
        }
    }
}