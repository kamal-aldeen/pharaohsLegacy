using System.ComponentModel.DataAnnotations;

namespace pharaohsLegacy.Models
{
    public class DailyFact
    {
        public int Id { get; set; }

        [Required]
        public string FactText { get; set; } = "";   // إنجليزي

        public string? FactTextAr { get; set; }        // عربي (nullable زي باقي الجداول)

        public string? Category { get; set; }          // اختياري: Daily Life / Science / Religion / Architecture...
        public string? CategoryAr { get; set; }         // الترجمة العربية للفئة — بتتكتب يدوي زي CategoryAr في Museum/Artifact/HistoricalEvent
    }
}