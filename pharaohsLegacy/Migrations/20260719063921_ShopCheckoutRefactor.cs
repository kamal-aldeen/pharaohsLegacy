using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pharaohsLegacy.Migrations
{
    /// <inheritdoc />
    public partial class ShopCheckoutRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "ShopOrders");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "ShopOrders");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "ShopOrders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Governorate",
                table: "ShopOrders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "ShopOrders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "ShippingFee",
                table: "ShopOrders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "ShopOrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShopOrderId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopOrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopOrderItems_ShopOrders_ShopOrderId",
                        column: x => x.ShopOrderId,
                        principalTable: "ShopOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShopOrderItems_ShopOrderId",
                table: "ShopOrderItems",
                column: "ShopOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShopOrderItems");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "ShopOrders");

            migrationBuilder.DropColumn(
                name: "Governorate",
                table: "ShopOrders");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "ShopOrders");

            migrationBuilder.DropColumn(
                name: "ShippingFee",
                table: "ShopOrders");

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "ShopOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "ShopOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
