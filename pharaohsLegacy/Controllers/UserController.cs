using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pharaohsLegacy.Models;
using pharaohsLegacy.Models.DTOs;
using pharaohsLegacy.ViewModels;

namespace pharaohsLegacy.Controllers
{
    public class UserController : Controller
    {
        private readonly AppDbContext context;

        public UserController(AppDbContext _context)
        {
            context = _context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "User");
        }

        public IActionResult Guest()
        {
            HttpContext.Session.SetString("UserEmail", "guest");
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginDTO login)
        {
            if (!ModelState.IsValid)
                return View();

            var user = context.Users
                .FirstOrDefault(u => u.Email == login.Email && u.Password == login.Password);

            if (user != null)
            {
                HttpContext.Session.SetString("UserEmail", user.Email);

                if (user.Email == "kamalabdlbast89@gmail.com")
                {
                    HttpContext.Session.SetString("UserRole", "Admin");
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    HttpContext.Session.SetString("UserRole", "User");
                    return RedirectToAction("Index", "Home");
                }
            }

            ViewBag.LoginError = "Invalid email or password";
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterDTO register)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ShowRegister = true;
                return View("Login");
            }

            bool emailExists = context.Users.Any(u => u.Email == register.Email);

            if (emailExists)
            {
                ViewBag.RegError = "This email is already registered";
                ViewBag.ShowRegister = true;
                return View("Login");
            }

            User user = new User
            {
                Name = register.Name,
                Email = register.Email,
                Password = register.Password
            };

            context.Users.Add(user);
            context.SaveChanges();

            return RedirectToAction("Login", "User");
        }


        public async Task<IActionResult> Dashboard(string tab = "overview")
        {
            var email = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(email) || email == "guest")
                return RedirectToAction("Login");

            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                return RedirectToAction("Login");

            // ===== BOOKINGS =====
            var bookings = await context.Bookings
                .Where(b => b.UserEmail == email && b.Status == "Confirmed")
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            var bookingCards = new List<BookingCardViewModel>();

            foreach (var b in bookings)
            {
                string placeName = b.PlaceName;
                string? imageUrl = null;

                if (b.PlaceType == "temple")
                {
                    var temple = await context.Temples.FindAsync(b.PlaceId);
                    placeName = temple?.Name ?? placeName;
                    imageUrl = temple?.ImageUrl;
                }
                else
                {
                    var museum = await context.Museums.FindAsync(b.PlaceId);
                    placeName = museum?.Name ?? placeName;
                    imageUrl = museum?.ImageUrl;
                }

                bookingCards.Add(new BookingCardViewModel
                {
                    Id = b.Id,
                    PlaceName = placeName,
                    PlaceType = b.PlaceType,
                    ImageUrl = imageUrl,
                    VisitDate = b.VisitDate,
                    NumberOfTickets = b.NumberOfTickets,
                    TotalPrice = b.TotalPrice,
                    Status = b.Status,
                    CreatedAt = b.CreatedAt
                });
            }

            // ===== FAVORITES =====
            var favorites = await context.Favorites
                .Where(f => f.UserEmail == email)
                .ToListAsync();

            var favPharaohs = new List<FavoriteCardViewModel>();
            var favTemples = new List<FavoriteCardViewModel>();
            var favGods = new List<FavoriteCardViewModel>();
            var favMuseums = new List<FavoriteCardViewModel>();
            var favArtifacts = new List<FavoriteCardViewModel>(); // ✅ جديد

            foreach (var fav in favorites)
            {
                if (fav.Type.ToLower() == "pharaoh")
                {
                    var p = await context.Pharaohs.FindAsync(fav.ItemId);
                    if (p != null)
                        favPharaohs.Add(new FavoriteCardViewModel
                        {
                            FavId = fav.Id,
                            ItemId = p.Id,
                            Name = p.Name,
                            Type = "pharaoh",
                            ImageUrl = p.ImageUrl,
                            SubTitle = p.Dynasty
                        });
                }
                else if (fav.Type.ToLower() == "temple")
                {
                    var t = await context.Temples.FindAsync(fav.ItemId);
                    if (t != null)
                        favTemples.Add(new FavoriteCardViewModel
                        {
                            FavId = fav.Id,
                            ItemId = t.Id,
                            Name = t.Name,
                            Type = "temple",
                            ImageUrl = t.ImageUrl,
                            SubTitle = t.Location
                        });
                }
                else if (fav.Type.ToLower() == "god")
                {
                    var g = await context.Gods.FindAsync(fav.ItemId);
                    if (g != null)
                        favGods.Add(new FavoriteCardViewModel
                        {
                            FavId = fav.Id,
                            ItemId = g.Id,
                            Name = g.Name,
                            Type = "god",
                            ImageUrl = g.ImageUrl,
                            SubTitle = g.Role
                        });
                }
                else if (fav.Type.ToLower() == "museum")
                {
                    var m = await context.Museums.FindAsync(fav.ItemId);
                    if (m != null)
                        favMuseums.Add(new FavoriteCardViewModel
                        {
                            FavId = fav.Id,
                            ItemId = m.Id,
                            Name = m.Name,
                            Type = "museum",
                            ImageUrl = m.ImageUrl,
                            SubTitle = m.Location
                        });
                }
                // ✅ Artifacts
                else if (fav.Type.ToLower() == "artifact")
                {
                    var a = await context.Artifacts.FindAsync(fav.ItemId);
                    if (a != null)
                        favArtifacts.Add(new FavoriteCardViewModel
                        {
                            FavId = fav.Id,
                            ItemId = a.Id,
                            Name = a.Name,
                            Type = "artifact",
                            ImageUrl = a.ImageUrl,
                            SubTitle = a.Category
                        });
                }
            }

            // ===== DASHBOARD VIEWMODEL =====
            var vm = new DashboardViewModel
            {
                UserName = user.Name,
                UserEmail = user.Email,
                TotalBookings = bookingCards.Count,
                ActiveBookings = bookingCards.Count(b => b.Status == "Confirmed"),
                TotalFavorites = favorites.Count,
                VisitedCount = bookingCards.Count(b => b.Status == "Visited"),
                TotalSpent = bookingCards
                                    .Where(b => b.Status != "Cancelled")
                                    .Sum(b => b.TotalPrice),
                Bookings = bookingCards,
                FavoritePharaohs = favPharaohs,
                FavoriteTemples = favTemples,
                FavoriteGods = favGods,
                FavoriteMuseums = favMuseums,
                FavoriteArtifacts = favArtifacts  // ✅ جديد
            };

            // ===== MY JOURNEY PINS =====
            var journeyPins = new List<JourneyPin>();

            var confirmedBookings = bookings
                .Where(b => b.Status == "Confirmed" || b.Status == "Visited")
                .ToList();

            foreach (var booking in confirmedBookings)
            {
                double lat = 0, lng = 0;
                string name = "", img = "", desc = "";

                if (booking.PlaceType?.ToLower() == "temple")
                {
                    var temple = await context.Temples.FindAsync(booking.PlaceId);
                    if (temple == null || temple.Latitude == 0) continue;
                    lat = temple.Latitude;
                    lng = temple.Longitude;
                    name = temple.Name;
                    img = temple.ImageUrl ?? "";
                    desc = temple.Description ?? "";
                }
                else if (booking.PlaceType?.ToLower() == "museum")
                {
                    var museum = await context.Museums.FindAsync(booking.PlaceId);
                    if (museum == null || museum.Latitude == 0) continue;
                    lat = museum.Latitude;
                    lng = museum.Longitude;
                    name = museum.Name;
                    img = museum.ImageUrl ?? "";
                    desc = museum.Description ?? "";
                }

                if (journeyPins.Any(p => p.ItemId == booking.PlaceId
                    && p.Type == booking.PlaceType?.ToLower()
                    && p.PinType == "booking")) continue;

                journeyPins.Add(new JourneyPin
                {
                    ItemId = booking.PlaceId,
                    Name = name,
                    Type = booking.PlaceType?.ToLower() ?? "",
                    PinType = booking.Status == "Visited" ? "Visited" : "booking",
                    Latitude = lat,
                    Longitude = lng,
                    ImageUrl = img,
                    Description = desc.Length > 100 ? desc[..100] + "..." : desc,
                    VisitDate = booking.VisitDate.ToString("dd MMM yyyy"),
                    Status = booking.Status ?? ""
                });
            }

            foreach (var fav in favorites.Where(f => f.Type.ToLower() == "temple"
                                                   || f.Type.ToLower() == "museum"))
            {
                bool alreadyBooked = journeyPins.Any(p => p.ItemId == fav.ItemId
                                                       && p.Type == fav.Type.ToLower());
                double lat = 0, lng = 0;
                string name = "", img = "", desc = "";

                if (fav.Type.ToLower() == "temple")
                {
                    var temple = await context.Temples.FindAsync(fav.ItemId);
                    if (temple == null || temple.Latitude == 0) continue;
                    lat = temple.Latitude;
                    lng = temple.Longitude;
                    name = temple.Name;
                    img = temple.ImageUrl ?? "";
                    desc = temple.Description ?? "";
                }
                else if (fav.Type.ToLower() == "museum")
                {
                    var museum = await context.Museums.FindAsync(fav.ItemId);
                    if (museum == null || museum.Latitude == 0) continue;
                    lat = museum.Latitude;
                    lng = museum.Longitude;
                    name = museum.Name;
                    img = museum.ImageUrl ?? "";
                    desc = museum.Description ?? "";
                }

                journeyPins.Add(new JourneyPin
                {
                    ItemId = fav.ItemId,
                    Name = name,
                    Type = fav.Type.ToLower(),
                    PinType = alreadyBooked ? "both" : "favorite",
                    Latitude = lat,
                    Longitude = lng,
                    ImageUrl = img,
                    Description = desc.Length > 100 ? desc[..100] + "..." : desc
                });
            }

            ViewBag.JourneyPins = journeyPins;
            ViewBag.JourneyCount = journeyPins.Count;

            ViewBag.ActiveTab = tab;
            return View(vm);
        }
    }
}