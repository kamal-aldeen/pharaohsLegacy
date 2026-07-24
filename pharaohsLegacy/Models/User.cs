using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace pharaohsLegacy.Models
{
    public class User:IdentityUser
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        // 🆕 Analytics Dashboard (بند 13) — تاريخ التسجيل، لازم لـ User Growth chart
        // اليوزرز القدام (قبل الـ Migration) هياخدوا تاريخ يوم تشغيل الـ Migration (GETDATE())
        [Column(TypeName = "datetime")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
