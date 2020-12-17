#region
using System;
using Microsoft.EntityFrameworkCore.Migrations;
#endregion

namespace Kudos.Migrations {
	// ReSharper disable once UnusedMember.Global
	public partial class InitialCreate : Migration {
		protected override void Down(MigrationBuilder migrationBuilder) {
			migrationBuilder.DropTable("HonorData");

			migrationBuilder.DropTable("QuestionData");
		}

		protected override void Up(MigrationBuilder migrationBuilder) {
			migrationBuilder.CreateTable("HonorData",
				table => new {
					Id = table.Column<ulong>(nullable: false).Annotation("Sqlite:Autoincrement", true),
					Honor = table.Column<int>(nullable: false),
					Honored = table.Column<ulong>(nullable: false),
					Honorer = table.Column<ulong>(nullable: false),
					Timestamp = table.Column<DateTime>(nullable: false)
				}, constraints: table => { table.PrimaryKey("PK_HonorData", x => x.Id); });

			migrationBuilder.CreateTable("QuestionData",
				table => new {
					Id = table.Column<ulong>(nullable: false),
					Answerer = table.Column<ulong>(nullable: false),
					Question = table.Column<string>(nullable: true),
					Questionnaire = table.Column<ulong>(nullable: false)
				}, constraints: table => { table.PrimaryKey("PK_QuestionData", x => x.Id); });
		}
	}
}
