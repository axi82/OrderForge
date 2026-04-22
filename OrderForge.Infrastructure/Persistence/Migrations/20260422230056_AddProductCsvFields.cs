using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderForge.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProductCsvFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "barcode",
                table: "products",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "commodity_code_description",
                table: "products",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "free_stock",
                table: "products",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "part_number",
                table: "products",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "product_code",
                table: "products",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "quantity_allocated",
                table: "products",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "quantity_in_stock",
                table: "products",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "quantity_on_order",
                table: "products",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "supplier_account_code",
                table: "products",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE products
                SET product_code = sku
                WHERE product_code IS NULL OR btrim(product_code) = '';
                """);

            migrationBuilder.AlterColumn<string>(
                name: "product_code",
                table: "products",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_products_product_code",
                table: "products",
                column: "product_code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_products_product_code",
                table: "products");

            migrationBuilder.DropColumn(
                name: "barcode",
                table: "products");

            migrationBuilder.DropColumn(
                name: "commodity_code_description",
                table: "products");

            migrationBuilder.DropColumn(
                name: "free_stock",
                table: "products");

            migrationBuilder.DropColumn(
                name: "part_number",
                table: "products");

            migrationBuilder.DropColumn(
                name: "product_code",
                table: "products");

            migrationBuilder.DropColumn(
                name: "quantity_allocated",
                table: "products");

            migrationBuilder.DropColumn(
                name: "quantity_in_stock",
                table: "products");

            migrationBuilder.DropColumn(
                name: "quantity_on_order",
                table: "products");

            migrationBuilder.DropColumn(
                name: "supplier_account_code",
                table: "products");
        }
    }
}
