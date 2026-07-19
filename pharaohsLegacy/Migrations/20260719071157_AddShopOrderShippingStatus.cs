using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pharaohsLegacy.Migrations
{
    /// <inheritdoc />
    public partial class AddShopOrderShippingStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShippingStatus",
                table: "ShopOrders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShippingStatus",
                table: "ShopOrders");
        }
    }
}
