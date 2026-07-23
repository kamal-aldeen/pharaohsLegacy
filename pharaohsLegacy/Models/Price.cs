using System.ComponentModel.DataAnnotations.Schema;

namespace pharaohsLegacy.Models
{
    public class Price
    {
        public int Id { get; set; }

        // "Temple" أو "Museum" — نفس قيم Booking.PlaceType بالظبط
        public string PlaceType { get; set; }

        // بيتربط بـ Temple.Id أو Museum.Id حسب PlaceType
        // (نفس أسلوب Favorites/Reviews — جدول نص وصفي، مش FK حقيقي)
        public int PlaceId { get; set; }

        // 🆕 اسم المكان (Temple.Name أو Museum.Name) — بيتخزن هنا كمان عشان
        // تبقى الجداول مفهومة لوحدها من غير ما تعمل Join كل مرة
        public string? PlaceName { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
    }
}
