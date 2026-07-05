using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace pharaohsLegacy.Migrations
{
    /// <inheritdoc />
    public partial class SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pharaohs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Dynasty = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Period = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pharaohs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Temples",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Period = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TicketUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Temples", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Pharaohs",
                columns: new[] { "Id", "Description", "Dynasty", "ImageUrl", "Name", "Period" },
                values: new object[,]
                {
                    { 1, "Known as Ramesses the Great, he reigned for 66 years and built the temples of Abu Simbel.", "19th Dynasty", "/images/pharaohs/ramesses2_child2.jpg", "Ramesses II", "1279–1213 BC" },
                    { 2, "The boy pharaoh whose intact tomb, discovered in 1922, revealed the splendors of ancient Egypt.", "18th Dynasty", "/images/pharaohs/tutankhamun_crop.jpg", "Tutankhamun", "1332–1323 BC" },
                    { 3, "The last active ruler of the Ptolemaic Kingdom of Egypt, known for her intelligence and alliances with Rome.", "Ptolemaic Dynasty", "/images/pharaohs/cleopatra.jpg", "Cleopatra VII", "51–30 BC" }
                });

            migrationBuilder.InsertData(
                table: "Temples",
                columns: new[] { "Id", "Description", "ImageUrl", "Location", "Name", "Period", "TicketUrl" },
                values: new object[,]
                {
                    { 1, "The largest ancient religious site in the world, dedicated to the god Amun.", "/images/temples/karnak3.jpg", "Luxor, Egypt", "Karnak Temple", "2055–100 BC", "https://egymonuments.gov.eg" },
                    { 2, "Two massive rock temples built by Ramesses II, relocated to avoid flooding from the Nile.", "/images/temples/abusimbel.jpg", "Aswan, Egypt", "Abu Simbel", "1264–1244 BC", "https://egymonuments.gov.eg" },
                    { 3, "A large ancient Egyptian temple complex located on the east bank of the Nile River.", "/images/temples/luxor2.jpg", "Luxor, Egypt", "Luxor Temple", "1400 BC", "https://egymonuments.gov.eg" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pharaohs");

            migrationBuilder.DropTable(
                name: "Temples");
        }
    }
}
