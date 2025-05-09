using MasterPiece.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MasterPiece.Controllers
{
	public class WishlistController : Controller
	{
		private readonly MyDbContext _context;

		public WishlistController(MyDbContext context)
		{
			_context = context;
		}

		public IActionResult MyWishlist()
		{
			var userId = HttpContext.Session.GetInt32("UserId");
			if (userId == null)
				return RedirectToAction("Login", "User");

			var wishlistItems = _context.Wishlists
				.Include(w => w.House)
				.Where(w => w.UserId == userId)
				.Select(w => w.House) // get the related house
				.ToList();

			return View(wishlistItems); // Pass list of House objects to the view
		}

		[HttpPost]
		public IActionResult AddToWishlist(int houseId)
		{
			var userId = HttpContext.Session.GetInt32("UserId");
			if (userId == null)
				return RedirectToAction("Login", "User");

			// Check if already exists
			bool alreadyExists = _context.Wishlists.Any(w => w.UserId == userId && w.HouseId == houseId);
			if (!alreadyExists)
			{
				_context.Wishlists.Add(new Wishlist
				{
					UserId = userId.Value,
					HouseId = houseId
				});
				_context.SaveChanges();
			}

			return RedirectToAction("Details", "Dar", new { id = houseId });
		}


		[HttpPost]
		public IActionResult RemoveFromWishlist(int houseId)
		{
			var userId = HttpContext.Session.GetInt32("UserId");
			if (userId == null)
				return RedirectToAction("Login", "User");

			var item = _context.Wishlists.FirstOrDefault(w => w.UserId == userId && w.HouseId == houseId);
			if (item != null)
			{
				_context.Wishlists.Remove(item);
				_context.SaveChanges();
			}

			return RedirectToAction("Index", "Dar", new { id = houseId });
		}



        [HttpPost]
        public IActionResult ToggleWishlist(int houseId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                TempData["NotLoggedIn"] = "Please log in to use the wishlist.";
                return RedirectToAction("Index", "Dar");
            }

            var existing = _context.Wishlists.FirstOrDefault(w => w.HouseId == houseId && w.UserId == userId);
            if (existing != null)
            {
                _context.Wishlists.Remove(existing);
            }
            else
            {
                _context.Wishlists.Add(new Wishlist { HouseId = houseId, UserId = userId.Value });
            }

            _context.SaveChanges();
            return RedirectToAction("Index", "Dar"); // Reload the list
        }



    }
}
