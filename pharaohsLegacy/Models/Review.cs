using System.ComponentModel.DataAnnotations;

namespace pharaohsLegacy.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Required]
        public string UserEmail { get; set; } = "";

        [Required]
        public string UserName { get; set; } = "";

        // pharaoh / temple / museum / god / artifact
        [Required]
        public string Type { get; set; } = "";

        public int ItemId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        [MaxLength(500)]
        public string Comment { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // ✅ Edit Review support
        public bool IsEdited { get; set; } = false;
    }
}