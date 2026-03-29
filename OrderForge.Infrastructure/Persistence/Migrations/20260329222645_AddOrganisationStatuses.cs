using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OrderForge.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganisationStatuses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "organisation_statuses",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organisation_statuses", x => x.id);
                });

            migrationBuilder.InsertData(
                table: "organisation_statuses",
                columns: new[] { "id", "code" },
                values: new object[,]
                {
                    { 1, "Active" },
                    { 2, "Inactive" },
                    { 3, "Unknown" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_organisation_statuses_code",
                table: "organisation_statuses",
                column: "code",
                unique: true);

            migrationBuilder.AddColumn<int>(
                name: "organisation_status_id",
                table: "organisations",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE organisations SET organisation_status_id = CASE status
                    WHEN 'Active' THEN 1
                    WHEN 'Inactive' THEN 2
                    WHEN 'Unknown' THEN 3
                    ELSE 3
                END
                """);

            migrationBuilder.Sql(
                """
                UPDATE organisations SET organisation_status_id = 1 WHERE organisation_status_id IS NULL
                """);

            migrationBuilder.AlterColumn<int>(
                name: "organisation_status_id",
                table: "organisations",
                type: "integer",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "status",
                table: "organisations");

            migrationBuilder.CreateIndex(
                name: "IX_organisations_organisation_status_id",
                table: "organisations",
                column: "organisation_status_id");

            migrationBuilder.AddForeignKey(
                name: "FK_organisations_organisation_statuses_organisation_status_id",
                table: "organisations",
                column: "organisation_status_id",
                principalTable: "organisation_statuses",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_organisations_organisation_statuses_organisation_status_id",
                table: "organisations");

            migrationBuilder.DropIndex(
                name: "IX_organisations_organisation_status_id",
                table: "organisations");

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "organisations",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Active");

            migrationBuilder.Sql(
                """
                UPDATE organisations o
                SET status = s.code
                FROM organisation_statuses s
                WHERE o.organisation_status_id = s.id
                """);

            migrationBuilder.DropColumn(
                name: "organisation_status_id",
                table: "organisations");

            migrationBuilder.DropTable(
                name: "organisation_statuses");
        }
    }
}
