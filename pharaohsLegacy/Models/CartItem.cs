using System.ComponentModel.DataAnnotations.Schema;

namespace pharaohsLegacy.Models
{
    // 🆕 عنصر في سلة المشتريات — مرتبط باليوزر (مش بالـ Session) عشان يفضل موجود لو خرج ورجع
    public class CartItem
    {
        public int Id { get; set; }
        public string UserEmail { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public DateTime AddedAt { get; set; }

        // نفس فلسفة [NotMapped] المستخدمة في ShopOrder — بيانات بتتجاب من Product وقت العرض بس
        [NotMapped]
        public string ProductName { get; set; } = "";

        [NotMapped]
        public string ProductImage { get; set; } = "";

        [NotMapped]
        public decimal ProductPrice { get; set; }

        [NotMapped]
        public decimal? ProductOriginalPrice { get; set; }

        [NotMapped]
        public int ProductStock { get; set; }
    }
}
