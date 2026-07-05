namespace pharaohsLegacy.Models
{
    public class Temple
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Period { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public string? NameAr { get; set; }
        public string? LocationAr { get; set; }
        public string? PeriodAr { get; set; }
        public string? DescriptionAr { get; set; }

    }
}