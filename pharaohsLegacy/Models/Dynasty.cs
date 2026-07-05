// Models/Dynasty.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pharaohsLegacy.Models
{
    public class Dynasty
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = "";          // "First Dynasty"

        [Required]
        public string Era { get; set; } = "";           // "Old Kingdom"

        public int StartYear { get; set; }              // -3100 = 3100 BC
        public int EndYear { get; set; }                // -2890

        [Required]
        public string Description { get; set; } = "";

        public string Achievements { get; set; } = "";  // إنجازات مميزة
        public string CapitalCity { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public string PharaohTag { get; set; } = ""; // e.g. "18th Dynasty"

        public string? NameAr { get; set; }
        public string? EraAr { get; set; }
        public string? DescriptionAr { get; set; }
        public string? AchievementsAr { get; set; }
        public string? CapitalCityAr { get; set; }
    }
}