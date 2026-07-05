namespace pharaohsLegacy.Models
{
    public class HistoricalEvent
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public int Year { get; set; } // سالب = BC
        public string Category { get; set; } = ""; // Political / Military / Religious / Cultural / Scientific
        public string Description { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public string? DynastyTag { get; set; } // يطابق Dynasty.Name
        public string? PharaohTag { get; set; } // يطابق Pharaoh.Name

        public string? TitleAr { get; set; }
        public string? CategoryAr { get; set; }
        public string? DescriptionAr { get; set; }

        public string YearLabel => Year < 0 ? $"{Math.Abs(Year)} BC" : $"{Year} AD";
    }
}