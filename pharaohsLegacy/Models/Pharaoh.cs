namespace pharaohsLegacy.Models
{
    public class Pharaoh
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Dynasty { get; set; }
        public string Period { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string? NameAr { get; set; }
        public string? DescriptionAr { get; set; }
        public string? DynastyAr { get; set; }
        public string? PeriodAr { get; set; }
    }
}