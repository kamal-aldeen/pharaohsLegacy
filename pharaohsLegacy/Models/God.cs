namespace pharaohsLegacy.Models
{
    public class God
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Role { get; set; } = "";
        public string Description { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public string Symbol { get; set; } = "";

        public string? NameAr { get; set; }
        public string? RoleAr { get; set; }
        public string? DescriptionAr { get; set; }
    }
}