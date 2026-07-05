namespace pharaohsLegacy.Models
{
    public class Museum
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Founded { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        
        public string Category { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public string? NameAr { get; set; }
        public string? LocationAr { get; set; }
        public string? DescriptionAr { get; set; }
        public string? CategoryAr { get; set; }
    }
}