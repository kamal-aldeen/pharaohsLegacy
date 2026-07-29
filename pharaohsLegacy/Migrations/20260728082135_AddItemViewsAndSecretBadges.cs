using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace pharaohsLegacy.Migrations
{
    /// <inheritdoc />
    public partial class AddItemViewsAndSecretBadges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ItemViews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserEmail = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    ViewedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemViews", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Category", "DescriptionAr", "DescriptionEn", "IconClass", "Key", "NameAr", "NameEn", "Threshold" },
                values: new object[] { "Knowledge", "استكشف 5 أسرات", "Explored 5 dynasties", "fa-solid fa-scroll", "dynasty_expert", "خبير الأسرات", "Dynasty Expert", 5 });

            migrationBuilder.UpdateData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Category", "DescriptionAr", "DescriptionEn", "IconClass", "Key", "NameAr", "NameEn", "Threshold" },
                values: new object[] { "Knowledge", "استكشف 15 أسرة", "Explored 15 dynasties", "fa-solid fa-scroll", "dynasty_expert", "خبير الأسرات", "Dynasty Expert", 15 });

            migrationBuilder.UpdateData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Category", "DescriptionAr", "DescriptionEn", "IconClass", "Key", "NameAr", "NameEn", "Threshold" },
                values: new object[] { "Knowledge", "استكشف 30 أسرة", "Explored 30 dynasties", "fa-solid fa-scroll", "dynasty_expert", "خبير الأسرات", "Dynasty Expert", 30 });

            migrationBuilder.UpdateData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "DescriptionAr", "DescriptionEn", "IconClass", "Key", "NameAr", "NameEn", "Threshold" },
                values: new object[] { "كتب 3 مراجعات", "Wrote 3 reviews", "fa-solid fa-pen", "reviewer", "كاتب مراجعات", "Reviewer", 3 });

            migrationBuilder.UpdateData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "DescriptionAr", "DescriptionEn", "IconClass", "Key", "NameAr", "NameEn", "Threshold" },
                values: new object[] { "كتب 10 مراجعات", "Wrote 10 reviews", "fa-solid fa-pen", "reviewer", "كاتب مراجعات", "Reviewer", 10 });

            migrationBuilder.UpdateData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "DescriptionAr", "DescriptionEn", "IconClass", "Key", "NameAr", "NameEn", "Threshold" },
                values: new object[] { "كتب 25 مراجعة", "Wrote 25 reviews", "fa-solid fa-pen", "reviewer", "كاتب مراجعات", "Reviewer", 25 });

            migrationBuilder.UpdateData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "Category", "DescriptionAr", "DescriptionEn", "IconClass", "Key", "NameAr", "NameEn" },
                values: new object[] { "Community", "حصل على 5 أصوات مفيدة", "Got 5 helpful votes", "fa-solid fa-handshake-angle", "community_helper", "مساعد المجتمع", "Community Helper" });

            migrationBuilder.UpdateData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "Category", "DescriptionAr", "DescriptionEn", "IconClass", "Key", "NameAr", "NameEn" },
                values: new object[] { "Community", "حصل على 20 صوت مفيد", "Got 20 helpful votes", "fa-solid fa-handshake-angle", "community_helper", "مساعد المجتمع", "Community Helper" });

            migrationBuilder.UpdateData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "Category", "DescriptionAr", "DescriptionEn", "IconClass", "Key", "NameAr", "NameEn" },
                values: new object[] { "Community", "حصل على 50 صوت مفيد", "Got 50 helpful votes", "fa-solid fa-handshake-angle", "community_helper", "مساعد المجتمع", "Community Helper" });

            migrationBuilder.UpdateData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "Category", "DescriptionAr", "DescriptionEn", "IconClass", "Key", "NameAr", "NameEn", "Threshold", "Tier" },
                values: new object[] { "Knowledge", "لعب 5 كويزات أو وصل لاستمرارية 3 أيام", "Played 5 quizzes or reached a 3-day streak", "fa-solid fa-brain", "quiz_master", "سيد الأسئلة", "Quiz Master", 5, "Bronze" });

            migrationBuilder.InsertData(
                table: "Badges",
                columns: new[] { "Id", "Category", "CreatedAt", "DescriptionAr", "DescriptionEn", "IconClass", "IsHidden", "Key", "NameAr", "NameEn", "Threshold", "Tier" },
                values: new object[,]
                {
                    { 17, "Knowledge", new DateTime(2026, 7, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "لعب 20 كويز أو وصل لاستمرارية 7 أيام", "Played 20 quizzes or reached a 7-day streak", "fa-solid fa-brain", false, "quiz_master", "سيد الأسئلة", "Quiz Master", 20, "Silver" },
                    { 18, "Knowledge", new DateTime(2026, 7, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "لعب 50 كويز أو وصل لاستمرارية 30 يوم", "Played 50 quizzes or reached a 30-day streak", "fa-solid fa-brain", false, "quiz_master", "سيد الأسئلة", "Quiz Master", 50, "Gold" },
                    { 19, "Legendary", new DateTime(2026, 7, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "حصل على الذهبي في كل شارة تانية", "Earned Gold in every other badge", "fa-solid fa-trophy", false, "legendary_explorer", "المستكشف الأسطوري", "Legendary Explorer", 0, "Gold" },
                    { 20, "Hidden", new DateTime(2026, 7, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "حقق 100% في كويز", "Scored 100% on a quiz", "fa-solid fa-star", true, "perfect_score", "الدرجة الكاملة", "Perfect Score", 1, "Gold" },
                    { 21, "Hidden", new DateTime(2026, 7, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "وصل لاستمرارية 30 يوم في الكويز", "Reached a 30-day quiz streak", "fa-solid fa-fire", true, "streak_legend", "أسطورة الاستمرارية", "Streak Legend", 30, "Gold" },
                    { 22, "Hidden", new DateTime(2026, 7, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "زار كل المتاحف", "Visited every museum", "fa-solid fa-building-columns", true, "museum_completionist", "جامع المتاحف", "Museum Completionist", 42, "Gold" },
                    { 23, "Hidden", new DateTime(2026, 7, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "استكشف كل الأسرات", "Explored every dynasty", "fa-solid fa-landmark-dome", true, "true_historian", "المؤرخ الحقيقي", "True Historian", 35, "Gold" },
                    { 24, "Hidden", new DateTime(2026, 7, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "عضو من سنة كاملة", "Member for a full year", "fa-solid fa-heart", true, "loyal_explorer", "المستكشف الوفي", "Loyal Explorer", 365, "Gold" },
                    { 25, "Hidden", new DateTime(2026, 7, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "حجز زيارة أو لعب كويز الساعة 3 الفجر", "Booked a visit or played a quiz at 3 AM", "fa-solid fa-moon", true, "night_owl", "بومة الليل", "Night Owl", 1, "Gold" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemViews_UserEmail_Type_ItemId",
                table: "ItemViews",
                columns: new[] { "UserEmail", "Type", "ItemId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemViews");

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.UpdateData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Category", "DescriptionAr", "DescriptionEn", "IconClass", "Key", "NameAr", "NameEn", "Threshold" },
                values: new object[] { "Community", "كتب 3 مراجعات", "Wrote 3 reviews", "fa-solid fa-pen", "reviewer", "كاتب مراجعات", "Reviewer", 3 });

            migrationBuilder.UpdateData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Category", "DescriptionAr", "DescriptionEn", "IconClass", "Key", "NameAr", "NameEn", "Threshold" },
                values: new object[] { "Community", "كتب 10 مراجعات", "Wrote 10 reviews", "fa-solid fa-pen", "reviewer", "كاتب مراجعات", "Reviewer", 10 });

            migrationBuilder.UpdateData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Category", "DescriptionAr", "DescriptionEn", "IconClass", "Key", "NameAr", "NameEn", "Threshold" },
                values: new object[] { "Community", "كتب 25 مراجعة", "Wrote 25 reviews", "fa-solid fa-pen", "reviewer", "كاتب مراجعات", "Reviewer", 25 });

            migrationBuilder.UpdateData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "DescriptionAr", "DescriptionEn", "IconClass", "Key", "NameAr", "NameEn", "Threshold" },
                values: new object[] { "حصل على 5 أصوات مفيدة", "Got 5 helpful votes", "fa-solid fa-handshake-angle", "community_helper", "مساعد المجتمع", "Community Helper", 5 });

            migrationBuilder.UpdateData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "DescriptionAr", "DescriptionEn", "IconClass", "Key", "NameAr", "NameEn", "Threshold" },
                values: new object[] { "حصل على 20 صوت مفيد", "Got 20 helpful votes", "fa-solid fa-handshake-angle", "community_helper", "مساعد المجتمع", "Community Helper", 20 });

            migrationBuilder.UpdateData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "DescriptionAr", "DescriptionEn", "IconClass", "Key", "NameAr", "NameEn", "Threshold" },
                values: new object[] { "حصل على 50 صوت مفيد", "Got 50 helpful votes", "fa-solid fa-handshake-angle", "community_helper", "مساعد المجتمع", "Community Helper", 50 });

            migrationBuilder.UpdateData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "Category", "DescriptionAr", "DescriptionEn", "IconClass", "Key", "NameAr", "NameEn" },
                values: new object[] { "Knowledge", "لعب 5 كويزات أو وصل لاستمرارية 3 أيام", "Played 5 quizzes or reached a 3-day streak", "fa-solid fa-brain", "quiz_master", "سيد الأسئلة", "Quiz Master" });

            migrationBuilder.UpdateData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "Category", "DescriptionAr", "DescriptionEn", "IconClass", "Key", "NameAr", "NameEn" },
                values: new object[] { "Knowledge", "لعب 20 كويز أو وصل لاستمرارية 7 أيام", "Played 20 quizzes or reached a 7-day streak", "fa-solid fa-brain", "quiz_master", "سيد الأسئلة", "Quiz Master" });

            migrationBuilder.UpdateData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "Category", "DescriptionAr", "DescriptionEn", "IconClass", "Key", "NameAr", "NameEn" },
                values: new object[] { "Knowledge", "لعب 50 كويز أو وصل لاستمرارية 30 يوم", "Played 50 quizzes or reached a 30-day streak", "fa-solid fa-brain", "quiz_master", "سيد الأسئلة", "Quiz Master" });

            migrationBuilder.UpdateData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "Category", "DescriptionAr", "DescriptionEn", "IconClass", "Key", "NameAr", "NameEn", "Threshold", "Tier" },
                values: new object[] { "Legendary", "حصل على الذهبي في كل شارة تانية", "Earned Gold in every other badge", "fa-solid fa-trophy", "legendary_explorer", "المستكشف الأسطوري", "Legendary Explorer", 0, "Gold" });
        }
    }
}
