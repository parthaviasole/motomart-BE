using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace motomart_BE.Data.Migrations
{
    public partial class RenameTablesToMoto : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "moto_orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    address_id = table.Column<Guid>(type: "uuid", nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    payment_type = table.Column<string>(type: "text", nullable: false),
                    otp = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_moto_orders", x => x.id);
                    table.ForeignKey(
                        name: "FK_moto_orders_addresses_address_id",
                        column: x => x.address_id,
                        principalTable: "addresses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_moto_orders_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "moto_order_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_moto_order_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_moto_order_items_moto_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "moto_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_moto_order_items_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "moto_cart_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_moto_cart_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_moto_cart_items_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_moto_cart_items_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_moto_cart_items_product_id",
                table: "moto_cart_items",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_moto_cart_items_user_id",
                table: "moto_cart_items",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_moto_order_items_order_id",
                table: "moto_order_items",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_moto_order_items_product_id",
                table: "moto_order_items",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_moto_orders_address_id",
                table: "moto_orders",
                column: "address_id");

            migrationBuilder.CreateIndex(
                name: "IX_moto_orders_user_id",
                table: "moto_orders",
                column: "user_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "moto_cart_items");
            migrationBuilder.DropTable(name: "moto_order_items");
            migrationBuilder.DropTable(name: "moto_orders");
        }
    }
}
