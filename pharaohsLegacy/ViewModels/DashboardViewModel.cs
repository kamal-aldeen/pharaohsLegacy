// ViewModels/DashboardViewModel.cs
using pharaohsLegacy.Models;

namespace pharaohsLegacy.ViewModels
{
    public class DashboardViewModel
    {
       
        public string UserName { get; set; } = "";
        public string UserEmail { get; set; } = "";

      
        public int TotalBookings { get; set; }
        public int ActiveBookings { get; set; }
        public int TotalFavorites { get; set; }
        public decimal TotalSpent { get; set; }
        public int VisitedCount { get; set; }

        public List<BookingCardViewModel> Bookings { get; set; } = new();

        
        public List<FavoriteCardViewModel> FavoritePharaohs { get; set; } = new();
        public List<FavoriteCardViewModel> FavoriteTemples { get; set; } = new();
        public List<FavoriteCardViewModel> FavoriteGods { get; set; } = new();
        public List<FavoriteCardViewModel> FavoriteMuseums { get; set; } = new();
        public List<FavoriteCardViewModel> FavoriteArtifacts { get; set; } = new();
        public List<FavoriteCardViewModel> FavoriteProducts { get; set; } = new();

    }

    public class BookingCardViewModel
    {
        public int Id { get; set; }
        public string PlaceName { get; set; } = "";
        public string PlaceType { get; set; } = "";  
        public string? ImageUrl { get; set; }
        public DateTime VisitDate { get; set; }
        public int NumberOfTickets { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }

    public class FavoriteCardViewModel
    {
        public int FavId { get; set; }
        public int ItemId { get; set; }
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";       
        public string? ImageUrl { get; set; }
        public string? SubTitle { get; set; }         
    }
}