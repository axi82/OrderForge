using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderForge.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddKeycloakOrganizationId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "keycloak_organization_id",
                table: "organisations",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_organisations_keycloak_organization_id",
                table: "organisations",
                column: "keycloak_organization_id",
                unique: true,
                filter: "keycloak_organization_id IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_organisations_keycloak_organization_id",
                table: "organisations");

            migrationBuilder.DropColumn(
                name: "keycloak_organization_id",
                table: "organisations");
        }
    }
}
