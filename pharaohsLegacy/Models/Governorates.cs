namespace pharaohsLegacy.Models
{
    // 🆕 جدول شحن ثابت في الكود — الأسهل للبداية بدل جدول داتا بيز (ممكن نحوّله بعدين
    // لو الأدمن احتاج يعدّل الأسعار من غير Deploy)
    public static class Governorates
    {
        public class GovernorateInfo
        {
            public string Key { get; set; } = "";
            public string NameEn { get; set; } = "";
            public string NameAr { get; set; } = "";
            public decimal ShippingFee { get; set; }

            // 🆕 عدد الأيام من لحظة الشحن (ShippedAt) لحد الوصول (Delivered) — نفس منطق أمازون
            public int DeliveryDays { get; set; }
        }

        public static readonly List<GovernorateInfo> All = new()
        {
            new() { Key = "Cairo",        NameEn = "Cairo",          NameAr = "القاهرة",       ShippingFee = 50,  DeliveryDays = 1 },
            new() { Key = "Giza",         NameEn = "Giza",           NameAr = "الجيزة",        ShippingFee = 50,  DeliveryDays = 1 },
            new() { Key = "Alexandria",   NameEn = "Alexandria",     NameAr = "الإسكندرية",     ShippingFee = 60,  DeliveryDays = 2 },
            new() { Key = "Qalyubia",     NameEn = "Qalyubia",       NameAr = "القليوبية",      ShippingFee = 55,  DeliveryDays = 2 },
            new() { Key = "PortSaid",     NameEn = "Port Said",      NameAr = "بورسعيد",        ShippingFee = 70,  DeliveryDays = 3 },
            new() { Key = "Suez",         NameEn = "Suez",           NameAr = "السويس",        ShippingFee = 70,  DeliveryDays = 3 },
            new() { Key = "Dakahlia",     NameEn = "Dakahlia",       NameAr = "الدقهلية",       ShippingFee = 60,  DeliveryDays = 2 },
            new() { Key = "Sharqia",      NameEn = "Sharqia",        NameAr = "الشرقية",        ShippingFee = 60,  DeliveryDays = 2 },
            new() { Key = "Gharbia",      NameEn = "Gharbia",        NameAr = "الغربية",        ShippingFee = 60,  DeliveryDays = 2 },
            new() { Key = "Monufia",      NameEn = "Monufia",        NameAr = "المنوفية",       ShippingFee = 55,  DeliveryDays = 2 },
            new() { Key = "Beheira",      NameEn = "Beheira",        NameAr = "البحيرة",        ShippingFee = 65,  DeliveryDays = 2 },
            new() { Key = "Ismailia",     NameEn = "Ismailia",       NameAr = "الإسماعيلية",     ShippingFee = 70,  DeliveryDays = 3 },
            new() { Key = "Faiyum",       NameEn = "Faiyum",         NameAr = "الفيوم",         ShippingFee = 65,  DeliveryDays = 2 },
            new() { Key = "BeniSuef",     NameEn = "Beni Suef",      NameAr = "بني سويف",       ShippingFee = 70,  DeliveryDays = 3 },
            new() { Key = "Minya",        NameEn = "Minya",          NameAr = "المنيا",         ShippingFee = 75,  DeliveryDays = 3 },
            new() { Key = "Asyut",        NameEn = "Asyut",          NameAr = "أسيوط",          ShippingFee = 80,  DeliveryDays = 3 },
            new() { Key = "Sohag",        NameEn = "Sohag",          NameAr = "سوهاج",          ShippingFee = 85,  DeliveryDays = 4 },
            new() { Key = "Qena",         NameEn = "Qena",           NameAr = "قنا",            ShippingFee = 90,  DeliveryDays = 4 },
            new() { Key = "Luxor",        NameEn = "Luxor",          NameAr = "الأقصر",         ShippingFee = 95,  DeliveryDays = 4 },
            new() { Key = "Aswan",        NameEn = "Aswan",          NameAr = "أسوان",          ShippingFee = 100, DeliveryDays = 5 },
            new() { Key = "RedSea",       NameEn = "Red Sea",        NameAr = "البحر الأحمر",    ShippingFee = 100, DeliveryDays = 5 },
            new() { Key = "NewValley",    NameEn = "New Valley",     NameAr = "الوادي الجديد",   ShippingFee = 110, DeliveryDays = 5 },
            new() { Key = "Matrouh",      NameEn = "Matrouh",        NameAr = "مطروح",          ShippingFee = 100, DeliveryDays = 5 },
            new() { Key = "NorthSinai",   NameEn = "North Sinai",    NameAr = "شمال سيناء",      ShippingFee = 110, DeliveryDays = 5 },
            new() { Key = "SouthSinai",   NameEn = "South Sinai",    NameAr = "جنوب سيناء",      ShippingFee = 110, DeliveryDays = 5 },
            new() { Key = "KafrElSheikh", NameEn = "Kafr El Sheikh", NameAr = "كفر الشيخ",       ShippingFee = 65,  DeliveryDays = 2 },
            new() { Key = "Damietta",     NameEn = "Damietta",       NameAr = "دمياط",          ShippingFee = 65,  DeliveryDays = 2 },
        };

        public static decimal GetFee(string? key) =>
            All.FirstOrDefault(g => g.Key == key)?.ShippingFee ?? 0;

        // 🆕 لو المفتاح مش موجود لأي سبب، نرجع 3 أيام كقيمة افتراضية معقولة بدل ما ننهار
        public static int GetDeliveryDays(string? key) =>
            All.FirstOrDefault(g => g.Key == key)?.DeliveryDays ?? 3;

        public static bool IsValid(string? key) =>
            !string.IsNullOrWhiteSpace(key) && All.Any(g => g.Key == key);
    }
}
