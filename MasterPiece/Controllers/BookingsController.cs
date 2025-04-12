using MasterPiece.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MasterPiece.Controllers
{
    public class BookingsController : Controller
    {
        private readonly MyDbContext _context;

        public BookingsController(MyDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var houseBookings = _context.HouseBookings
                .Include(b => b.User)
                .Include(b => b.House)
                .ToList();

            var tourBookings = _context.TourBookings
                .Include(b => b.User)
                .Include(b => b.Tour)
                .ToList();

            ViewBag.TourBookings = tourBookings;
            return View(houseBookings);
        }

        [HttpPost]
        public IActionResult CancelHouse(int id)
        {
            var booking = _context.HouseBookings.Find(id);
            if (booking != null)
            {
                booking.Status = "Cancelled";
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult CancelTour(int id)
        {
            var booking = _context.TourBookings.Find(id);
            if (booking != null)
            {
                booking.Status = "Cancelled";
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
