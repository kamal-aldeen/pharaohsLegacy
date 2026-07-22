using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pharaohsLegacy.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketTokenToBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TicketToken",
                table: "Bookings",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TicketToken",
                table: "Bookings");
        }
    }
}
