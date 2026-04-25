using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OrderForge.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTradeOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "trade_orders",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    organisation_id = table.Column<int>(type: "integer", nullable: false),
                    order_number = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    placed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trade_orders", x => x.id);
                    table.ForeignKey(
                        name: "FK_trade_orders_organisations_organisation_id",
                        column: x => x.organisation_id,
                        principalTable: "organisations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "trade_order_lines",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    trade_order_id = table.Column<int>(type: "integer", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    line_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trade_order_lines", x => x.id);
                    table.ForeignKey(
                        name: "FK_trade_order_lines_trade_orders_trade_order_id",
                        column: x => x.trade_order_id,
                        principalTable: "trade_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_trade_order_lines_trade_order_id_sort_order",
                table: "trade_order_lines",
                columns: new[] { "trade_order_id", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "IX_trade_orders_organisation_id_placed_at",
                table: "trade_orders",
                columns: new[] { "organisation_id", "placed_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trade_order_lines");

            migrationBuilder.DropTable(
                name: "trade_orders");
        }
    }
}
