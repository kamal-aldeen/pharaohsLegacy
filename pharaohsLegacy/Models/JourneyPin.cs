namespace pharaohsLegacy.Models
{
    public class JourneyPin
    {
        public int ItemId { get; set; }
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";      // "temple" or "museum"
        public string PinType { get; set; } = "";   // "booking" or "favorite"
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string ImageUrl { get; set; } = "";
        public string Description { get; set; } = "";
        public string VisitDate { get; set; } = ""; // for bookings only
        public string Status { get; set; } = "";    // for bookings only
        public string? NameAr { get; set; }
        public string? DescriptionAr { get; set; }
    }
}