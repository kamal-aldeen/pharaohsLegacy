using System.Linq;
using Microsoft.AspNetCore.Mvc;
using pharaohsLegacy.Models;

namespace pharaohsLegacy.Controllers
{
    // بند 15 — Notification System
    public class NotificationController : Controller
    {
        private readonly AppDbContext _context;

        public NotificationController(AppDbContext context)
        {
            _context = context;
        }

        // GET /Notification/Index — الصفحة الكاملة لكل الإشعارات
        public IActionResult Index()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "User");

            var notifications = _context.Notifications
                .Where(n => n.UserEmail == email)
                .OrderByDescending(n => n.CreatedAt)
                .ToList();

            return View(notifications);
        }

        // GET /Notification/GetUnreadCount — للـ Polling (Navbar bell)
        [HttpGet]
        public IActionResult GetUnreadCount()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email))
                return Json(new { count = 0 });

            var count = _context.Notifications.Count(n => n.UserEmail == email && !n.IsRead);
            return Json(new { count });
        }

        // GET /Notification/GetRecent — لآخر 5 إشعارات في الـ Dropdown
        [HttpGet]
        public IActionResult GetRecent()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email))
                return Json(new { items = new object[0] });

            var items = _context.Notifications
                .Where(n => n.UserEmail == email)
                .OrderByDescending(n => n.CreatedAt)
                .Take(5)
                .Select(n => new
                {
                    n.Id,
                    n.Title,
                    n.Message,
                    n.IsRead,
                    n.Link,
                    CreatedAt = n.CreatedAt.ToString("dd MMM, hh:mm tt")
                })
                .ToList();

            return Json(new { items });
        }

        // POST /Notification/MarkAsRead/5
        [HttpPost]
        public IActionResult MarkAsRead(int id)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            var notification = _context.Notifications
                .FirstOrDefault(n => n.Id == id && n.UserEmail == email);

            if (notification != null)
            {
                notification.IsRead = true;
                _context.SaveChanges();
            }

            return Ok();
        }

        // POST /Notification/MarkAllAsRead
        [HttpPost]
        public IActionResult MarkAllAsRead()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email))
                return Ok();

            var unread = _context.Notifications
                .Where(n => n.UserEmail == email && !n.IsRead)
                .ToList();

            foreach (var n in unread)
                n.IsRead = true;

            _context.SaveChanges();
            return Ok();
        }
    }
}
