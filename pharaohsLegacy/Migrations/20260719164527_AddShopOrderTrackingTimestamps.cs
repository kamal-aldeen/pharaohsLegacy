using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pharaohsLegacy.Migrations
{
    /// <inheritdoc />
    public partial class AddShopOrderTrackingTimestamps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ConfirmedAt",
                table: "ShopOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveredAt",
                table: "ShopOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ShippedAt",
                table: "ShopOrders",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfirmedAt",
                table: "ShopOrders");

            migrationBuilder.DropColumn(
                name: "DeliveredAt",
                table: "ShopOrders");

            migrationBuilder.DropColumn(
                name: "ShippedAt",
                table: "ShopOrders");
        }
    }
}
