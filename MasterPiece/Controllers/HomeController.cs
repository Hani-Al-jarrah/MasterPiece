//using MasterPiece.Models;
//using Microsoft.AspNetCore.Mvc;
//using System.Diagnostics;

//namespace MasterPiece.Controllers
//{
//    public class HomeController : Controller
//    {
//        private readonly ILogger<HomeController> _logger;
//		private readonly MyDbContext _context;


//		public HomeController(ILogger<HomeController> logger , MyDbContext context)
//        {
//            _logger = logger;
//			_context = context;
//		}



//		public IActionResult Index()
//        {
//            return View();
//        }
//		public IActionResult About()
//		{
//			return View();
//		}
//		public IActionResult Contact()
//		{
//			return View();
//		}
//		public IActionResult Tours()
//		{
//			var tours = _context.Tours.ToList();
//			// Fetch all tours from DB
//			return View(tours); // Pass to view
//		}

//		public IActionResult Privacy()
//        {
//            return View();
//        }

//        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
//        public IActionResult Error()
//        {
//            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
//        }



//		public IActionResult Dar()
//		{
//			return View();
//		}
//	}
//}


using MasterPiece.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Mail;
using System.Net;

namespace MasterPiece.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly MyDbContext _context;

		public HomeController(ILogger<HomeController> logger, MyDbContext context)
		{
			_logger = logger;
			_context = context;
		}
        public IActionResult Index()
        {
            var houses = _context.Houses.Take(5).ToList();
            var tours = _context.Tours.Take(4).ToList();

            ViewBag.Houses = houses;
            ViewBag.Tours = tours;

            return View();
        }

        public IActionResult About()
		{
			var userCount = _context.Users.Count();
			ViewBag.UserCount = userCount;
			return View();
		}

		public IActionResult Contact()
		{
			return View();
		}

		[HttpPost]
		public IActionResult Contact(string name_contact, string lastname_contact, string email_contact, string phone_contact, string message_contact)
		{
			Console.WriteLine($"Name: {name_contact}");
			Console.WriteLine($"Email: {email_contact}");
			Console.WriteLine($"Message: {message_contact}");
			try
			{
				// Basic server-side validation
				if (string.IsNullOrWhiteSpace(name_contact) ||
		string.IsNullOrWhiteSpace(lastname_contact) ||
		string.IsNullOrWhiteSpace(email_contact) ||
		string.IsNullOrWhiteSpace(message_contact))
				{
					TempData["Error"] = "Please fill in all required fields.";
					return RedirectToAction("Contact");
				}

				var fullName = $"{name_contact} {lastname_contact}";

				// Save to DB
				var contact = new ContactU
				{
					FullName = fullName,
					Email = email_contact,
					Subject = $"Phone: {phone_contact}",
					Message = message_contact,
					CreatedAt = DateTime.Now
				};

				_context.ContactUs.Add(contact);
				_context.SaveChanges();

				// Send email to admin
				var mail = new MailMessage();
				mail.From = new MailAddress("hanijarrah20@gmail.com"); // sender
				mail.To.Add("hanijarrah20@gmail.com"); // recipient
				mail.Subject = "New Contact Us Message";
				mail.Body = $"From: {fullName}\nEmail: {email_contact}\nPhone: {phone_contact}\nMessage:\n{message_contact}";

				var smtp = new SmtpClient("smtp.gmail.com", 587)
				{
					Credentials = new NetworkCredential("hanijarrah20@gmail.com", "bfjy ycst hihg mlin"),
					EnableSsl = true
				};

				smtp.Send(mail);

				TempData["Success"] = "Your message has been sent successfully!";
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to send contact message");
				TempData["Error"] = "Something went wrong while sending your message. Please try again later.";
			}

			return RedirectToAction("Contact");
		}


		public IActionResult Tours()
		{
			var tours = _context.Tours.ToList();
			return View(tours);
		}

		public IActionResult RecentAdventures()
		{
			var adventures = _context.Tours
				.OrderByDescending(t => t.Id)
				.Skip(5)
				.Take(8)
				.ToList();

			return View(adventures);
		}


		public IActionResult TourDetails(int id)
		{
            ViewBag.AllUsers = _context.Users.ToList(); // For profile pictures

            var tour = _context.Tours.FirstOrDefault(t => t.Id == id);
			if (tour == null) return NotFound();

            var images = _context.Images.FirstOrDefault(i => i.TourId == id);
            var reviews = _context.Feedbacks
				.Where(r => r.TourId == id)
				.OrderByDescending(r => r.CreatedAt)
				.ToList();

			var totalGuests = _context.TourBookings
				.Where(b => b.TourId == id && b.Status != "Cancelled")
				.Sum(b => b.Guests);

			var remainingSpots = (tour.MaxGuests ?? 0) - totalGuests;
            ViewBag.Images = images;
            ViewBag.Reviews = reviews;
			ViewBag.RemainingSpots = remainingSpots;
			ViewBag.UserId = HttpContext.Session.GetInt32("UserId");
			ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");

			return View(tour);
		}


        [HttpPost]
        public IActionResult SubmitTourReview(IFormCollection form)
        {
            try
            {
                int userId = (int)HttpContext.Session.GetInt32("UserId");
                int tourId = int.Parse(form["TourId"]);
                int rating = int.Parse(form["Rating"]);
                string email = form["Email"];
                string comment = form["Comment"];

                var review = new Feedback
                {
                    UserId = userId,
                    TourId = tourId,
                    Rating = rating,
                    Email = email,
                    Comment = comment,
                    CreatedAt = DateTime.Now
                };

                _context.Feedbacks.Add(review);
                _context.SaveChanges();

                var allReviews = _context.Feedbacks.Where(f => f.TourId == tourId).ToList();
                var count = allReviews.Count;
                var avg = allReviews.Any() ? Math.Round((decimal)allReviews.Average(r => r.Rating), 1) : 0;

                return Json(new
                {
                    success = true,
                    count,
                    avg,
                    review = new
                    {
                        rating = review.Rating,
                        email = review.Email,
                        comment = review.Comment,
                        createdAt = review.CreatedAt
                    }
                });
            }
            catch
            {
                return Json(new { success = false });
            }
        }















        public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}

		public IActionResult Dar()
		{
			return View();
		}
	}
}
