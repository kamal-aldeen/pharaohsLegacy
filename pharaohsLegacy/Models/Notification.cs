using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pharaohsLegacy.Models
{
    // بند 15 — Notification System
    // جدول واحد لكل الإشعارات (يوزر عادي + أدمن) — الأدمن بيتفلتر بـ UserEmail الثابت بتاعه
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        public string UserEmail { get; set; } = "";

        [Required]
        public string Title { get; set; } = "";

        [Required]
        public string Message { get; set; } = "";

        // Booking / Review / Quiz / System / Admin
        [Required]
        public string Type { get; set; } = "System";

        public bool IsRead { get; set; } = false;

        [Column(TypeName = "datetime")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // اللينك اللي هيتودّى له اليوزر لو ضغط على الإشعار (مثلاً /Booking/MyBookings)
        // Nullable — مش كل إشعار لازم يكون له لينك
        public string? Link { get; set; }
    }
}
