using MasterPiece.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace MasterPiece.Controllers
{
    public class DarController : Controller
    {
        private readonly MyDbContext _context;

        public DarController(MyDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string? terms, string? location, List<int>? stars, DateTime? availableFrom, int? minPrice, int? maxPrice, int? guests, string? sortBy)
        {
            var houses = _context.Houses.AsQueryable();

            if (!string.IsNullOrEmpty(terms))
                houses = houses.Where(h => h.Name.Contains(terms) || h.Description.Contains(terms));

            if (!string.IsNullOrEmpty(location))
                houses = houses.Where(h => h.LocationName == location);

            if (stars?.Any() == true)
                houses = houses.Where(h => h.Stars.HasValue && stars.Contains(h.Stars.Value));




            if (minPrice.HasValue)
                houses = houses.Where(h => h.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                houses = houses.Where(h => h.Price <= maxPrice.Value);

            if (guests.HasValue)
                houses = houses.Where(h => h.MaxGuests >= guests.Value);

            // Sorting
            houses = sortBy switch
            {
                "priceLow" => houses.OrderBy(h => h.Price),
                "priceHigh" => houses.OrderByDescending(h => h.Price),
                "rating" => houses.OrderByDescending(h => h.Rate),
                _ => houses.OrderBy(h => h.Name)
            };

            return View(houses.ToList());
        }





        public IActionResult Details(int id)
        {
            var house = _context.Houses.FirstOrDefault(h => h.Id == id);
            if (house == null)
                return NotFound();

            var images = _context.Images.FirstOrDefault(i => i.HouseId == id);
            var reviews = _context.Feedbacks
                .Where(f => f.HouseId == id)
                .OrderByDescending(f => f.CreatedAt)
                .ToList();

            ViewBag.Images = images;
            ViewBag.Reviews = reviews;

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId != null)
            {
                var user = _context.Users.FirstOrDefault(u => u.Id == userId);
                ViewBag.UserEmail = user?.Email;
            }
			bool isInWishlist = false;

			if (userId != null)
			{
				isInWishlist = _context.Wishlists.Any(w => w.UserId == userId && w.HouseId == id);
			}


			// In DarController.cs (Details action)
			var bookedDates = _context.HouseBookings
	.Where(b => b.HouseId == id)
	.ToList()
	.SelectMany(b =>
	{
		var     checkIn = b.CheckInDate.Value.ToDateTime(TimeOnly.MinValue);
		var checkOut = b.CheckOutDate.Value.ToDateTime(TimeOnly.MinValue);

		return Enumerable.Range(0, (checkOut - checkIn).Days)
						 .Select(offset => checkIn.AddDays(offset).ToString("yyyy-MM-dd"));
	})
	.ToList();

			// Null-safe conversion
			ViewBag.AvailableFrom = house.AvailableFrom.HasValue
				? house.AvailableFrom.Value.ToDateTime(TimeOnly.MinValue).ToString("yyyy-MM-dd")
				: null;

			ViewBag.AvailableTo = house.AvailableTo.HasValue
				? house.AvailableTo.Value.ToDateTime(TimeOnly.MinValue).ToString("yyyy-MM-dd")
				: null;

			ViewBag.BookedDates = bookedDates;





			ViewBag.IsInWishlist = isInWishlist;

			return View(house);
        }

		/////////



		[HttpPost]
		public IActionResult SubmitHouseReviewAjax([FromForm] Feedback review)
		{
			if (!ModelState.IsValid || string.IsNullOrWhiteSpace(review.Comment) || review.Rating < 1)
			{
				return Json(new { success = false, message = "Invalid input." });
			}

			review.TourId = null;
			review.CreatedAt = DateTime.Now;

			_context.Feedbacks.Add(review);
			_context.SaveChanges();

			return Json(new { success = true });
		}





	}
}
