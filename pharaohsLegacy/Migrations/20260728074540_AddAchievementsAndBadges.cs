using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace pharaohsLegacy.Migrations
{
    /// <inheritdoc />
    public partial class AddAchievementsAndBadges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Badges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DescriptionEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DescriptionAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IconClass = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsHidden = table.Column<bool>(type: "bit", nullable: false),
                    Threshold = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Badges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserBadges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BadgeId = table.Column<int>(type: "int", nullable: false),
                    EarnedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Seen = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBadges", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Badges",
                columns: new[] { "Id", "Category", "CreatedAt", "DescriptionAr", "DescriptionEn", "IconClass", "IsHidden", "Key", "NameAr", "NameEn", "Threshold", "Tier" },
                values: new object[,]
                {
                    { 1, "Visit", new DateTime(2026, 7, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "زار 3 أماكن", "Visited 3 places", "fa-solid fa-compass", false, "explorer", "مستكشف", "Explorer", 3, "Bronze" },
                    { 2, "Visit", new DateTime(2026, 7, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "زار 7 أماكن", "Visited 7 places", "fa-solid fa-compass", false, "explorer", "سيد المعابد", "Temple Master", 7, "Silver" },
                    { 3, "Visit", new DateTime(2026, 7, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "زار 15 مكان", "Visited 15 places", "fa-solid fa-compass", false, "explorer", "المستكشف الأعظم", "Grand Explorer", 15, "Gold" },
                    { 4, "Knowledge", new DateTime(2026, 7, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "أضاف 5 فراعنة للمفضلة", "Favorited 5 pharaohs", "fa-solid fa-crown", false, "pharaoh_expert", "خبير الفراعنة", "Pharaoh Expert", 5, "Bronze" },
                    { 5, "Knowledge", new DateTime(2026, 7, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "أضاف 15 فرعون للمفضلة", "Favorited 15 pharaohs", "fa-solid fa-crown", false, "pharaoh_expert", "خبير الفراعنة", "Pharaoh Expert", 15, "Silver" },
                    { 6, "Knowledge", new DateTime(2026, 7, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "أضاف 30 فرعون للمفضلة", "Favorited 30 pharaohs", "fa-solid fa-crown", false, "pharaoh_expert", "خبير الفراعنة", "Pharaoh Expert", 30, "Gold" },
                    { 7, "Community", new DateTime(2026, 7, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "كتب 3 مراجعات", "Wrote 3 reviews", "fa-solid fa-pen", false, "reviewer", "كاتب مراجعات", "Reviewer", 3, "Bronze" },
                    { 8, "Community", new DateTime(2026, 7, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "كتب 10 مراجعات", "Wrote 10 reviews", "fa-solid fa-pen", false, "reviewer", "كاتب مراجعات", "Reviewer", 10, "Silver" },
                    { 9, "Community", new DateTime(2026, 7, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "كتب 25 مراجعة", "Wrote 25 reviews", "fa-solid fa-pen", false, "reviewer", "كاتب مراجعات", "Reviewer", 25, "Gold" },
                    { 10, "Community", new DateTime(2026, 7, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "حصل على 5 أصوات مفيدة", "Got 5 helpful votes", "fa-solid fa-handshake-angle", false, "community_helper", "مساعد المجتمع", "Community Helper", 5, "Bronze" },
                    { 11, "Community", new DateTime(2026, 7, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "حصل على 20 صوت مفيد", "Got 20 helpful votes", "fa-solid fa-handshake-angle", false, "community_helper", "مساعد المجتمع", "Community Helper", 20, "Silver" },
                    { 12, "Community", new DateTime(2026, 7, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "حصل على 50 صوت مفيد", "Got 50 helpful votes", "fa-solid fa-handshake-angle", false, "community_helper", "مساعد المجتمع", "Community Helper", 50, "Gold" },
                    { 13, "Knowledge", new DateTime(2026, 7, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "لعب 5 كويزات أو وصل لاستمرارية 3 أيام", "Played 5 quizzes or reached a 3-day streak", "fa-solid fa-brain", false, "quiz_master", "سيد الأسئلة", "Quiz Master", 5, "Bronze" },
                    { 14, "Knowledge", new DateTime(2026, 7, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "لعب 20 كويز أو وصل لاستمرارية 7 أيام", "Played 20 quizzes or reached a 7-day streak", "fa-solid fa-brain", false, "quiz_master", "سيد الأسئلة", "Quiz Master", 20, "Silver" },
                    { 15, "Knowledge", new DateTime(2026, 7, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "لعب 50 كويز أو وصل لاستمرارية 30 يوم", "Played 50 quizzes or reached a 30-day streak", "fa-solid fa-brain", false, "quiz_master", "سيد الأسئلة", "Quiz Master", 50, "Gold" },
                    { 16, "Legendary", new DateTime(2026, 7, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "حصل على الذهبي في كل شارة تانية", "Earned Gold in every other badge", "fa-solid fa-trophy", false, "legendary_explorer", "المستكشف الأسطوري", "Legendary Explorer", 0, "Gold" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Badges");

            migrationBuilder.DropTable(
                name: "UserBadges");
        }
    }
}
