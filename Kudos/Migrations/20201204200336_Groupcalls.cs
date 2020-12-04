#region
using Microsoft.EntityFrameworkCore.Migrations;
#endregion

namespace Kudos.Migrations {
	// ReSharper disable once UnusedMember.Global
	public partial class GroupCalls : Migration {
		protected override void Down(MigrationBuilder migrationBuilder) {
			migrationBuilder.DropTable("GroupData");
		}

		protected override void Up(MigrationBuilder migrationBuilder) {
			migrationBuilder.CreateTable("GroupData",
				table => new {
					ChannelId = table.Column<ulong>(nullable: false).Annotation("Sqlite:Autoincrement", true),
					Auto = table.Column<bool>(nullable: false),
					UserIds = table.Column<string>(nullable: true)
				}, constraints: table => { table.PrimaryKey("PK_GroupData", x => x.ChannelId); });
		}
	}
}
