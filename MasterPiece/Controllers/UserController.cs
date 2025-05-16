using MasterPiece.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MasterPiece.Controllers
{
	public class UserController : Controller
	{

		private readonly MyDbContext _context;


		public UserController(MyDbContext context)
		{
			_context = context;

		}


		public IActionResult Login()
		{
			return View();
		}

		// ---------- LOGIN METHOD ----------
		[HttpPost]
		public IActionResult Login(string Email, string Password)
		{
			// Find user by email
			var user = _context.Users.FirstOrDefault(u => u.Email == Email);

			if (Email == "Admin@gmail.com" && Password == "Admin") 
			{ return RedirectToAction("Index", "Admin"); }

			if (user != null && user.Password == Password) // No hashing, normal password check
			{
				// Store user details in session (allowing null values)
				HttpContext.Session.SetInt32("UserId", user.Id);
				HttpContext.Session.SetString("UserName", user.FullName ?? "Unknown User");
				HttpContext.Session.SetString("UserRole", user.Role ?? "User");
				HttpContext.Session.SetString("UserImage", user.ImageUrl ?? "default.png");

				TempData["SuccessMessage"] = "Login successful! Welcome to DarNa.";
				return RedirectToAction("Index", "Home"); // Redirect to the homepage
			}

			// Show error if login fails
			TempData["ErrorMessage"] = "Invalid email or password.";
			return RedirectToAction("Login");
		}

		public IActionResult Logout()
		{
			// Clear session data
			HttpContext.Session.Clear();
			return RedirectToAction("Login", "User");
		}

		public IActionResult Register()
		{
			return View();
		}


		// ---------- REGISTER METHOD ----------
		//[HttpPost]
		//public IActionResult Register(User user)
		//{
		//	// Check if passwords match
		//	if (user.Password != user.ConfirmPassword)
		//	{
		//		TempData["ErrorMessage"] = "Passwords do not match!";
		//		return RedirectToAction("Register");
		//	}

		//	// Check if the email already exists
		//	var existingUser = _context.Users.FirstOrDefault(u => u.Email == user.Email);
		//	if (existingUser != null)
		//	{
		//		TempData["ErrorMessage"] = "This email is already registered.";
		//		return RedirectToAction("Register");
		//	}

		//	// Save the user directly without checking ModelState
		//	user.Role = "User"; // Default role

		//	_context.Users.Add(user);
		//	_context.SaveChanges();

		//	TempData["SuccessMessage"] = "Registration successful! Please log in.";
		//	return RedirectToAction("Login");
		//}
		//[HttpPost]
		//	public IActionResult Register(User user)
		//	{
		//		if (user.Password != user.ConfirmPassword)
		//		{
		//			ViewBag.ErrorMessage = "Passwords do not match!";
		//			return View(user);
		//		}

		//		var existingUser = _context.Users.FirstOrDefault(u => u.Email == user.Email);
		//		if (existingUser != null)
		//		{
		//			ViewBag.ErrorMessage = "This email is already registered.";
		//			return View(user);
		//		}

		//		user.Role = "User";
		//		_context.Users.Add(user);
		//		_context.SaveChanges();

		//		ViewBag.SuccessMessage = "Registration successful! Please log in.";
		//		return RedirectToAction("Login");
		//	}


		[HttpPost]
		public IActionResult Register(string FullName, string Email, string Password, string ConfirmPassword, string PhoneNumber)
		{
			if (Password != ConfirmPassword)
			{
				ViewBag.ErrorMessage = "Passwords do not match!";
				return View();
			}

			var existingUser = _context.Users.FirstOrDefault(u => u.Email == Email);
			if (existingUser != null)
			{
				ViewBag.ErrorMessage = "This email is already registered.";
				return View();
			}

			var user = new User
			{
				FullName = FullName,
				Email = Email,
				Password = Password,
				PhoneNumber = PhoneNumber,
				Role = "User"
			};

			_context.Users.Add(user);
			_context.SaveChanges();

			ViewBag.SuccessMessage = "Registration successful! Please log in.";
			return RedirectToAction("Login");
		}


        //public IActionResult Profile()
        //{
        //    var userId = HttpContext.Session.GetInt32("UserId");
        //    if (userId == null)
        //    {
        //        return RedirectToAction("Login", "User");
        //    }

        //    var user = _context.Users.Find(userId);
        //    if (user == null)
        //    {
        //        return NotFound();
        //    }

        //    var houseBookings = _context.HouseBookings
        //        .Include(h => h.House)
        //        .Where(h => h.UserId == userId)
        //        .ToList();

        //    var tourBookings = _context.TourBookings
        //        .Include(t => t.Tour)
        //        .Where(t => t.UserId == userId)
        //        .ToList();

        //    var wishlist = _context.Wishlists
        //        .Include(w => w.House)
        //        .Where(w => w.UserId == userId)
        //        .ToList();

        //    var payments = _context.Payments
        //        .Where(p => p.UserId == userId)
        //        .ToList();

        //    ViewBag.User = user;
        //    ViewBag.HouseBookings = houseBookings;
        //    ViewBag.TourBookings = tourBookings;
        //    ViewBag.Wishlist = wishlist;
        //    ViewBag.Payments = payments;

        //    return View();
        //}
        public IActionResult Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "User");

            var user = _context.Users.Find(userId);
            if (user == null)
                return NotFound();

            var houseBookings = _context.HouseBookings
                .Include(h => h.House)
                .Where(h => h.UserId == userId)
                .ToList();

            var tourBookings = _context.TourBookings
                .Include(t => t.Tour)
                .Where(t => t.UserId == userId)
                .ToList();

            bool anyTourCancelled = false;
            bool anyHouseCancelled = false;

            // Auto-cancel expired tours
            foreach (var booking in tourBookings)
            {
                var tourDate = booking.Tour?.StartDate;
                if ((booking.Status == "Pending" || booking.Status == "Confirmed") &&
                    tourDate < DateOnly.FromDateTime(DateTime.Today))
                {
                    booking.Status = "Cancelled";
                    anyTourCancelled = true;
                }
            }

            // Auto-cancel expired houses
            foreach (var booking in houseBookings)
            {
                if ((booking.Status == "Pending" || booking.Status == "Confirmed") &&
                    booking.CheckInDate < DateOnly.FromDateTime(DateTime.Today))
                {
                    booking.Status = "Cancelled";
                    anyHouseCancelled = true;
                }
            }

            if (anyTourCancelled || anyHouseCancelled)
                _context.SaveChanges();

            if (anyTourCancelled)
                TempData["CancelledTours"] = "Some of your tour bookings were automatically canceled due to missed payment deadlines.";

            if (anyHouseCancelled)
                TempData["CancelledHouses"] = "Some of your house bookings were automatically canceled due to missed payment deadlines.";

            var wishlist = _context.Wishlists
                .Include(w => w.House)
                .Where(w => w.UserId == userId)
                .ToList();

            //var payments = _context.Payments
            //    .Where(p => p.UserId == userId)
            //    .ToList();
            var payments = _context.Payments
    .Where(p => p.UserId == userId)
    .Include(p => p.Booking).ThenInclude(b => b.House)
    .Include(p => p.TourBooking).ThenInclude(tb => tb.Tour)
    .ToList();


            ViewBag.User = user;
            ViewBag.HouseBookings = houseBookings;
            ViewBag.TourBookings = tourBookings;
            ViewBag.Wishlist = wishlist;
            ViewBag.Payments = payments;

            return View();
        }


        [HttpGet]
        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ResetPassword(string CurrentPassword, string NewPassword, string ConfirmPassword)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login");

            var user = _context.Users.Find(userId);
            if (user == null)
                return NotFound();

            if (user.Password != CurrentPassword)
            {
                TempData["Error"] = "Current password is incorrect.";
                return RedirectToAction("Profile");
            }

            if (NewPassword != ConfirmPassword)
            {
                TempData["Error"] = "Passwords do not match.";
                return RedirectToAction("Profile");
            }

            user.Password = NewPassword; // In a real-world app, hash the password
            _context.SaveChanges();

            TempData["Success"] = "Password updated successfully!";
            return RedirectToAction("Profile");
        }



        [HttpGet]
        public IActionResult EditProfile()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "User");

            var user = _context.Users.Find(userId);
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(User updatedUser, IFormFile ImageFile)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == updatedUser.Id);
            if (user == null)
                return NotFound();

            // Check if the email is already taken by another user
            var emailExists = _context.Users.Any(u => u.Email == updatedUser.Email && u.Id != updatedUser.Id);
            if (emailExists)
            {
                TempData["Error"] = "This email is already taken.";
                return RedirectToAction("Profile");
            }

            user.FullName = updatedUser.FullName;
            user.PhoneNumber = updatedUser.PhoneNumber;
            user.Email = updatedUser.Email;

            if (ImageFile != null && ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "UserImages");
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + ImageFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                user.ImageUrl = "/UserImages/" + uniqueFileName;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Profile updated!";
            return RedirectToAction("Profile");
        }

    }
}
