//using Microsoft.AspNetCore.Mvc;

//namespace MasterPiece.Controllers
//{
//    public class AdminController : Controller
//    {
//        public IActionResult Index()
//        {
//            return View();
//        }
//    }
//}
using MasterPiece.Models;
using Microsoft.AspNetCore.Mvc;

namespace MasterPiece.Controllers
{
    public class AdminController : Controller
    {
        private readonly MyDbContext _context;

        public AdminController(MyDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
			if (HttpContext.Session.GetString("UserRole") != "Admin")
			{
				return RedirectToAction("Login", "User");
			}
			var stats = new
            {
                Houses = _context.Houses.Count(),
                Tours = _context.Tours.Count(),
                Users = _context.Users.Count(),
                Contacts = _context.ContactUs.Count(),
                Feedbacks = _context.Feedbacks.Count(),
                Bookings = _context.HouseBookings.Count() + _context.TourBookings.Count(),
                PaymentsCount = _context.Payments.Count(),
                PaymentsTotal = _context.Payments.Sum(p => (decimal?)p.Amount) ?? 0
            };

            return View(stats);
        }
    }
}
