using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pharaohsLegacy.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTicketUrlAndWebsiteUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TicketUrl",
                table: "Temples");

            migrationBuilder.DropColumn(
                name: "WebsiteUrl",
                table: "Museums");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TicketUrl",
                table: "Temples",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WebsiteUrl",
                table: "Museums",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Temples",
                keyColumn: "Id",
                keyValue: 1,
                column: "TicketUrl",
                value: "https://egymonuments.gov.eg");

            migrationBuilder.UpdateData(
                table: "Temples",
                keyColumn: "Id",
                keyValue: 2,
                column: "TicketUrl",
                value: "https://egymonuments.gov.eg");

            migrationBuilder.UpdateData(
                table: "Temples",
                keyColumn: "Id",
                keyValue: 3,
                column: "TicketUrl",
                value: "https://egymonuments.gov.eg");
        }
    }
}
