using System.ComponentModel.DataAnnotations.Schema;

namespace pharaohsLegacy.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? NameAr { get; set; }        // 🆕 زي باقي الجداول (Temples/Museums/Gods) — عشان يدعم اللغتين
        public string Description { get; set; }
        public string? DescriptionAr { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public int StockQuantity { get; set; }

        public int? CategoryId { get; set; }        // 🆕 nullable — منتج ممكن يفضل من غير تصنيف

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }      // 🆕 navigation property

        // 🆕 المواصفات — كلها nullable، نفس نمط الحقول التانية (فولباك للـ EN لو الـ AR فاضية)
        public string? Material { get; set; }
        public string? MaterialAr { get; set; }
        public string? Dimensions { get; set; }
        public string? DimensionsAr { get; set; }
        public string? OriginRegion { get; set; }
        public string? OriginRegionAr { get; set; }

        // 🆕 المرحلة 3 — عروض وخصومات وشارات
        // OriginalPrice: nullable — لو موجودة (وأكبر من Price) بتبان Strikethrough + نسبة خصم محسوبة في الـ View
        [Column(TypeName = "decimal(18,2)")]
        public decimal? OriginalPrice { get; set; }

        public bool IsFeatured { get; set; }    // شارة "مميز" — بتتحط يدويًا من الأدمن
        public bool IsBestSeller { get; set; }  // شارة "الأكثر مبيعًا" — بتتحط يدويًا من الأدمن
        public bool IsNew { get; set; }         // شارة "جديد" — بتتحط يدويًا من الأدمن (مفيش CreatedAt في الموديل لسه عشان تتحسب تلقائي)
    }
}
