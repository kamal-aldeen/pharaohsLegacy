using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pharaohsLegacy.Migrations
{
    /// <inheritdoc />
    public partial class AddArabicTranslations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DescriptionAr",
                table: "Temples",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocationAr",
                table: "Temples",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "Temples",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PeriodAr",
                table: "Temples",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionAr",
                table: "Pharaohs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DynastyAr",
                table: "Pharaohs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "Pharaohs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PeriodAr",
                table: "Pharaohs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CategoryAr",
                table: "Museums",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionAr",
                table: "Museums",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocationAr",
                table: "Museums",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "Museums",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CategoryAr",
                table: "HistoricalEvents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionAr",
                table: "HistoricalEvents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleAr",
                table: "HistoricalEvents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionAr",
                table: "Gods",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "Gods",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RoleAr",
                table: "Gods",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AchievementsAr",
                table: "Dynasties",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CapitalCityAr",
                table: "Dynasties",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionAr",
                table: "Dynasties",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EraAr",
                table: "Dynasties",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "Dynasties",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CategoryAr",
                table: "Artifacts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrentLocationAr",
                table: "Artifacts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionAr",
                table: "Artifacts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "Artifacts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginAr",
                table: "Artifacts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PeriodAr",
                table: "Artifacts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Pharaohs",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DescriptionAr", "DynastyAr", "NameAr", "PeriodAr" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Pharaohs",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "DescriptionAr", "DynastyAr", "NameAr", "PeriodAr" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Pharaohs",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "DescriptionAr", "DynastyAr", "NameAr", "PeriodAr" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Temples",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DescriptionAr", "LocationAr", "NameAr", "PeriodAr" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Temples",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "DescriptionAr", "LocationAr", "NameAr", "PeriodAr" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Temples",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "DescriptionAr", "LocationAr", "NameAr", "PeriodAr" },
                values: new object[] { null, null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DescriptionAr",
                table: "Temples");

            migrationBuilder.DropColumn(
                name: "LocationAr",
                table: "Temples");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "Temples");

            migrationBuilder.DropColumn(
                name: "PeriodAr",
                table: "Temples");

            migrationBuilder.DropColumn(
                name: "DescriptionAr",
                table: "Pharaohs");

            migrationBuilder.DropColumn(
                name: "DynastyAr",
                table: "Pharaohs");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "Pharaohs");

            migrationBuilder.DropColumn(
                name: "PeriodAr",
                table: "Pharaohs");

            migrationBuilder.DropColumn(
                name: "CategoryAr",
                table: "Museums");

            migrationBuilder.DropColumn(
                name: "DescriptionAr",
                table: "Museums");

            migrationBuilder.DropColumn(
                name: "LocationAr",
                table: "Museums");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "Museums");

            migrationBuilder.DropColumn(
                name: "CategoryAr",
                table: "HistoricalEvents");

            migrationBuilder.DropColumn(
                name: "DescriptionAr",
                table: "HistoricalEvents");

            migrationBuilder.DropColumn(
                name: "TitleAr",
                table: "HistoricalEvents");

            migrationBuilder.DropColumn(
                name: "DescriptionAr",
                table: "Gods");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "Gods");

            migrationBuilder.DropColumn(
                name: "RoleAr",
                table: "Gods");

            migrationBuilder.DropColumn(
                name: "AchievementsAr",
                table: "Dynasties");

            migrationBuilder.DropColumn(
                name: "CapitalCityAr",
                table: "Dynasties");

            migrationBuilder.DropColumn(
                name: "DescriptionAr",
                table: "Dynasties");

            migrationBuilder.DropColumn(
                name: "EraAr",
                table: "Dynasties");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "Dynasties");

            migrationBuilder.DropColumn(
                name: "CategoryAr",
                table: "Artifacts");

            migrationBuilder.DropColumn(
                name: "CurrentLocationAr",
                table: "Artifacts");

            migrationBuilder.DropColumn(
                name: "DescriptionAr",
                table: "Artifacts");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "Artifacts");

            migrationBuilder.DropColumn(
                name: "OriginAr",
                table: "Artifacts");

            migrationBuilder.DropColumn(
                name: "PeriodAr",
                table: "Artifacts");
        }
    }
}
