
using pharaohsLegacy.Models;

namespace pharaohsLegacy.ViewModels
{
    public class AdminOverviewViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalPharaohs { get; set; }
        public int TotalTemples { get; set; }
        public int TotalMuseums { get; set; }
        public int TotalBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalVisited { get; set; }
        public int TotalGods { get; set; }
        public int TotalArtifacts { get; set; }
        public int TotalReviews { get; set; }
        public int TotalDynasties { get; set; }
        public int TotalHistoricalEvents { get; set; }
        //عملت initialize للقوائم لتجنب Null Reference أثناء عرض البيانات في الـ View.
        public List<Pharaoh> Pharaohs { get; set; } = new();
        public List<Temple> Temples { get; set; } = new();
        public List<Museum> Museums { get; set; } = new();
        public List<God> Gods { get; set; } = new();
        public List<User> Users { get; set; } = new();

        public List<Artifact> Artifacts { get; set; } = new();

        
        public List<Review> Reviews { get; set; } = new();

        public List<ReviewReport> Reports { get; set; } = new();

        
        public List<Dynasty> Dynasties { get; set; } = new();

        
        public List<HistoricalEvent> HistoricalEvents { get; set; } = new();


        //استخدمتش Booking model

        //لأن جدول الحجز الأصلي غالبًا مش فيه كل البيانات اللي محتاج تعرضها في الـ Dashboard.


        public List<AdminBookingRow> Bookings { get; set; } = new();
        public int TotalFacts { get; set; }
        public List<DailyFact> Facts { get; set; }

        public int TotalProducts { get; set; }
        public List<Product> Products { get; set; }
        public int TotalShopOrders { get; set; }
        public decimal TotalShopRevenue { get; set; }

        public int TotalCategories { get; set; }
        public List<Category> Categories { get; set; }

        public List<ShopOrder> ShopOrders { get; set; }

        // 🆕 Analytics Dashboard (بند 13)
        public List<RevenuePoint> RevenueTrend { get; set; } = new();
        public List<PlaceBookingCount> TopBookedPlaces { get; set; } = new();
        public List<UserGrowthPoint> UserGrowth { get; set; } = new();
        public ReviewsSummary ReviewsStats { get; set; } = new();
        public QuizSummary QuizStats { get; set; } = new();

    }

    // ──────────────────────────────
    // 🆕 Analytics Dashboard DTOs (بند 13)
    // ──────────────────────────────

    public class RevenuePoint
    {
        public string Label { get; set; } = "";
        public decimal BookingRevenue { get; set; }
        public decimal ShopRevenue { get; set; }
    }

    public class PlaceBookingCount
    {
        public string PlaceName { get; set; } = "";
        public string PlaceType { get; set; } = "";
        public int Count { get; set; }
    }

    public class UserGrowthPoint
    {
        public string Label { get; set; } = "";
        public int NewUsers { get; set; }
    }

    public class TypeRatingAvg
    {
        public string Type { get; set; } = "";
        public double AverageRating { get; set; }
    }

    public class ReviewsSummary
    {
        public double OverallAverageRating { get; set; }
        public string TopRatedName { get; set; } = "-";
        public double TopRatedAvg { get; set; }
        public string LowestRatedName { get; set; } = "-";
        public double LowestRatedAvg { get; set; }
        public List<TypeRatingAvg> AverageByType { get; set; } = new();
    }

    public class GradeCount
    {
        public string Grade { get; set; } = "";
        public int Count { get; set; }
    }

    public class QuizSummary
    {
        public int TotalPlayers { get; set; }
        public int TotalPlays { get; set; }
        public double AverageScorePercent { get; set; }
        public double AverageStreakDays { get; set; }
        public List<GradeCount> GradeDistribution { get; set; } = new();
    }

    public class AdminBookingRow
    {
        public int Id { get; set; }
        public string UserEmail { get; set; } = "";
        public string PlaceName { get; set; } = "";
        public string PlaceType { get; set; } = "";
        public DateTime VisitDate { get; set; }
        public int NumberOfTickets { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }
}