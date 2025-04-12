using MasterPiece.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MasterPiece.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly MyDbContext _context;

        public FeedbackController(MyDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var feedback = _context.Feedbacks
                .Include(f => f.User)
                .Include(f => f.House)
                .Include(f => f.Tour)
                .OrderByDescending(f => f.CreatedAt)
                .ToList();

            return View(feedback);
        }
    }
}
