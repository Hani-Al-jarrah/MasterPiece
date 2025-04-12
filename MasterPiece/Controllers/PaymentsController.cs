using MasterPiece.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MasterPiece.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly MyDbContext _context;

        public PaymentsController(MyDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var payments = _context.Payments
      .Include(p => p.User)
      .Include(p => p.Booking).ThenInclude(b => b.House)
      .Include(p => p.TourBooking).ThenInclude(b => b.Tour)
      .OrderByDescending(p => p.PaymentDate)
      .ToList();


            return View(payments);
        }
    }
}
