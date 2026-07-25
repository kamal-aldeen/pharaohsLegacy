using System;
using pharaohsLegacy.Models;

namespace pharaohsLegacy.Services
{
    // بند 15 — Notification System
    // Helper بسيط (مش DI Service كامل عشان يفضل سهل الاستخدام من أي Controller
    // بنفس روح باقي المشروع). بينادى بـ: NotificationHelper.Create(_context, ...)
    public static class NotificationHelper
    {
        public const string AdminEmail = "kamalabdlbast89@gmail.com";

        public static void Create(
            AppDbContext context,
            string userEmail,
            string title,
            string message,
            string type,
            string? link = null)
        {
            var notification = new Notification
            {
                UserEmail = userEmail,
                Title = title,
                Message = message,
                Type = type,
                Link = link,
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            context.Notifications.Add(notification);
            // ⚠️ متعملش SaveChanges هنا — سيبها للـ Controller اللي بينادي
            // عشان لو جوه نفس الـ SaveChanges بتاع الحجز/الريفيو نفسه (transaction واحدة)
        }

        // Shortcut لإشعار الأدمن (مثلاً حجز جديد يستنى مراجعة)
        public static void NotifyAdmin(
            AppDbContext context,
            string title,
            string message,
            string type = "Admin",
            string? link = null)
        {
            Create(context, AdminEmail, title, message, type, link);
        }
    }
}
