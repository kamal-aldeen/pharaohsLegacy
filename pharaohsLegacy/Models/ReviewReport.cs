namespace pharaohsLegacy.Models
{
    public class ReviewReport
    {
        public int Id { get; set; }
        public int ReviewId { get; set; }
        public string ReporterEmail { get; set; } = "";
        public string Reason { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsResolved { get; set; } = false;
    }
}