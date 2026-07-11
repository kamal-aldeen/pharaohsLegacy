using Microsoft.AspNetCore.Mvc;
using pharaohsLegacy.Models;
using Microsoft.EntityFrameworkCore;

namespace pharaohsLegacy.Controllers
{
    public class BookingController : Controller
    {
        private readonly AppDbContext _db;
        public BookingController(AppDbContext db) => _db = db;

        public async Task<IActionResult> MyBookings()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail) || userEmail == "guest")
                return RedirectToAction("Login", "User");

            var bookings = await _db.Bookings
                .Where(b => b.UserEmail == userEmail)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            var lang = HttpContext.Session.GetString("Lang") ?? "en";

            foreach (var b in bookings)
            {
                if (b.PlaceType == "Temple")
                {
                    var temple = await _db.Temples.FindAsync(b.PlaceId);
                    b.PlaceName = (lang == "ar" && !string.IsNullOrEmpty(temple?.NameAr))
                        ? temple.NameAr
                        : (temple?.Name ?? "معبد غير معروف");
                }
                else if (b.PlaceType == "Museum")
                {
                    var museum = await _db.Museums.FindAsync(b.PlaceId);
                    b.PlaceName = (lang == "ar" && !string.IsNullOrEmpty(museum?.NameAr))
                        ? museum.NameAr
                        : (museum?.Name ?? "متحف غير معروف");
                }
            }

            return View(bookings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
                return Content("❌ NO SESSION");

            var booking = await _db.Bookings
                .FirstOrDefaultAsync(b => b.Id == id && b.UserEmail == userEmail);
            if (booking == null)
                return Content($"❌ NOT FOUND — id={id} | email={userEmail}");

            if (booking.Status != "Confirmed")
                return Content($"❌ STATUS = {booking.Status}");

            if ((DateTime.Now - booking.CreatedAt).TotalHours > 48)
                return Content($"❌ EXPIRED — CreatedAt={booking.CreatedAt}");

            booking.Status = "Cancelled";
            var payment = await _db.Payments.FirstOrDefaultAsync(p => p.BookingId == id);
            if (payment != null) payment.Status = "Refunded";
            await _db.SaveChangesAsync();

            return RedirectToAction("Dashboard", "User", new { tab = "bookings" });
        }

        public async Task<IActionResult> Create(string placeType, int placeId)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail) || userEmail == "guest")
                return RedirectToAction("Login", "User");

            
            if (HttpContext.Session.GetString("UserRole") == "Admin")
            {
                TempData["Error"] = "Admins cannot make bookings!";
                return RedirectToAction("Index", "Home");
            }

            string? placeName = null;
            string? placeImage = null;
            var lang = HttpContext.Session.GetString("Lang") ?? "en";

            if (placeType == "Temple")
            {
                var temple = await _db.Temples.FindAsync(placeId);
                placeName = (lang == "ar" && !string.IsNullOrEmpty(temple?.NameAr)) ? temple.NameAr : temple?.Name;
                placeImage = temple?.ImageUrl;
            }
            else if (placeType == "Museum")
            {
                var museum = await _db.Museums.FindAsync(placeId);
                placeName = (lang == "ar" && !string.IsNullOrEmpty(museum?.NameAr)) ? museum.NameAr : museum?.Name;
                placeImage = museum?.ImageUrl;
            }

            if (placeName == null)
                return NotFound();

            ViewBag.PlaceType = placeType;
            ViewBag.PlaceId = placeId;
            ViewBag.PlaceName = placeName;
            ViewBag.PlaceImage = placeImage;
            ViewBag.TicketPrice = placeType == "Temple" ? 150 : 100;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(Booking booking)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail) || userEmail == "guest")
                return RedirectToAction("Login", "User");

            
            if (HttpContext.Session.GetString("UserRole") == "Admin")
            {
                TempData["Error"] = "Admins cannot make bookings!";
                return RedirectToAction("Index", "Home");
            }

            if (booking.VisitDate < DateTime.Today.AddDays(1) ||
                booking.VisitDate > DateTime.Today.AddMonths(1))
            {
                return RedirectToAction("Create", new
                {
                    placeType = booking.PlaceType,
                    placeId = booking.PlaceId
                });
            }

            booking.UserEmail = userEmail;
            booking.Status = "Confirmed";
            booking.CreatedAt = DateTime.Now;

            int ticketPrice = booking.PlaceType == "Temple" ? 150 : 100;
            booking.TotalPrice = ticketPrice * booking.NumberOfTickets;

            _db.Bookings.Add(booking);
            await _db.SaveChangesAsync();

            var payment = new Payment
            {
                BookingId = booking.Id,
                Amount = booking.TotalPrice,
                PaymentDate = DateTime.Now,
                PaymentMethod = Request.Form["PaymentMethod"].ToString(),
                Status = "Completed"
            };
            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();

            return RedirectToAction("MyBookings");
        }
    }
}