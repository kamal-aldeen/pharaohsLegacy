namespace pharaohsLegacy.Models
{
    public class Artifact
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Origin { get; set; } = "";
        public string Period { get; set; } = "";
        public string Category { get; set; } = "";
        public string Description { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public string Museum { get; set; } = "";
        public string CurrentLocation { get; set; } = "";

        public string? NameAr { get; set; }
        public string? MuseumAr { get; set; }
        public string? OriginAr { get; set; }
        public string? PeriodAr { get; set; }
        public string? CategoryAr { get; set; }
        public string? DescriptionAr { get; set; }
        public string? CurrentLocationAr { get; set; }
    }
}