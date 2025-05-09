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

		[HttpPost]
		public JsonResult SubmitHouseReviewAjax(int HouseId, int Rating, string Comment)
		{
			var userId = HttpContext.Session.GetInt32("UserId");
			if (userId == null) return Json(new { success = false });

			var user = _context.Users.FirstOrDefault(u => u.Id == userId);
			if (user == null) return Json(new { success = false });

			var review = new Feedback
			{
				HouseId = HouseId,
				UserId = user.Id,
				Rating = Rating,
				Comment = Comment,
				Email = user.Email,
				CreatedAt = DateTime.Now
			};

			_context.Feedbacks.Add(review);
			_context.SaveChanges();

			// Update house average rating
			var approvedReviews = _context.Feedbacks.Where(f => f.HouseId == HouseId).ToList();
			var newAvg = approvedReviews.Average(r => r.Rating);

			var house = _context.Houses.Find(HouseId);
			if (house != null)
			{
				house.Rate = Math.Round((decimal)newAvg, 1);
				_context.SaveChanges();
			}

			return Json(new { success = true });
		}

	}
}
