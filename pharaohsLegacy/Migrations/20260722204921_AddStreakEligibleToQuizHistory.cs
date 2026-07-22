using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pharaohsLegacy.Migrations
{
    /// <inheritdoc />
    public partial class AddStreakEligibleToQuizHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "StreakEligible",
                table: "QuizHistories",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StreakEligible",
                table: "QuizHistories");
        }
    }
}
