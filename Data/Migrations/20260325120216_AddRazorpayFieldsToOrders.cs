using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace motomart_BE.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRazorpayFieldsToOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "payment_status",
                table: "moto_orders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "razorpay_order_id",
                table: "moto_orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "razorpay_payment_id",
                table: "moto_orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "razorpay_signature",
                table: "moto_orders",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "payment_status",
                table: "moto_orders");

            migrationBuilder.DropColumn(
                name: "razorpay_order_id",
                table: "moto_orders");

            migrationBuilder.DropColumn(
                name: "razorpay_payment_id",
                table: "moto_orders");

            migrationBuilder.DropColumn(
                name: "razorpay_signature",
                table: "moto_orders");
        }
    }
}
