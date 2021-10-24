#region

using Microsoft.EntityFrameworkCore.Migrations;

#endregion

namespace Kudos.Migrations {

    // ReSharper disable once UnusedMember.Global
    public partial class Bans : Migration {

        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropTable("BanData");
        }

        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.CreateTable("BanData",
                table => new {
                    UserId = table.Column<ulong>(nullable: false).Annotation("Sqlite:Autoincrement", true),
                    HardBan = table.Column<bool>(nullable: false)
                }, constraints: table => { table.PrimaryKey("PK_BanData", x => x.UserId); });
        }
    }
}