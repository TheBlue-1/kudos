#region
using Microsoft.EntityFrameworkCore.Migrations;
#endregion

namespace Kudos.Migrations {
	// ReSharper disable once UnusedMember.Global
	public partial class AddServerCallRoles : Migration {
		protected override void Down(MigrationBuilder migrationBuilder) {
			migrationBuilder.DropColumn("RoleIds", "GroupData");
		}

		protected override void Up(MigrationBuilder migrationBuilder) {
			migrationBuilder.AddColumn<string>("RoleIds", "GroupData", nullable: true);
		}
	}
}
