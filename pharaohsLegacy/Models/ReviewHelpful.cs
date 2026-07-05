namespace pharaohsLegacy.Models
{
    public class ReviewHelpful
    {
        public int Id { get; set; }
        public int ReviewId { get; set; }
        public string UserEmail { get; set; } = "";
    }
}