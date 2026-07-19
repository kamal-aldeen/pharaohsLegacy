using System.ComponentModel.DataAnnotations.Schema;

namespace pharaohsLegacy.Models
{
    // 🆕 عنصر داخل أوردر — الأوردر بقى ممكن يحتوي أكتر من منتج (من السلة) بدل منتج واحد بس
    public class ShopOrderItem
    {
        public int Id { get; set; }

        public int ShopOrderId { get; set; }
        [ForeignKey("ShopOrderId")]
        public ShopOrder? ShopOrder { get; set; }

        public int ProductId { get; set; }
        public int Quantity { get; set; }

        // سعر الوحدة وقت الشراء — بنسجله هنا (Snapshot) عشان لو سعر المنتج اتغيّر بعدين
        // في الأدمن، الأوردرات القديمة تفضل تعرض السعر اللي فعليًا اتدفع بيه
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [NotMapped]
        public string ProductName { get; set; } = "";

        [NotMapped]
        public string ProductImage { get; set; } = "";
    }
}
