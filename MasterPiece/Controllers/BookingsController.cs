using MasterPiece.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;

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









		//[HttpPost]
		//public IActionResult BookHouse(int HouseID, string BookingDates, int Guests)
		//{
		//	var userId = HttpContext.Session.GetInt32("UserId");
		//	if (userId == null)
		//		return RedirectToAction("Login", "User");

		//	// Parse Dates
		//	var dates = BookingDates?.Split(" to ");
		//	if (dates == null || dates.Length != 2 ||
		//		!DateTime.TryParse(dates[0], out DateTime checkIn) ||
		//		!DateTime.TryParse(dates[1], out DateTime checkOut))
		//	{
		//		TempData["Error"] = "Invalid booking dates.";
		//		return RedirectToAction("Details", "Dar", new { id = HouseID });
		//	}

		//	// Validate House
		//	var house = _context.Houses.FirstOrDefault(h => h.Id == HouseID);
		//	if (house == null || house.Available != true || Guests > house.MaxGuests)
		//	{
		//		TempData["Error"] = "House not available or guest count exceeded.";
		//		return RedirectToAction("Details", "Dar", new { id = HouseID });
		//	}

		//	// Check if selected dates are within house availability
		//	var availableFrom = house.AvailableFrom?.ToDateTime(TimeOnly.MinValue);
		//	var availableTo = house.AvailableTo?.ToDateTime(TimeOnly.MinValue);
		//	if (checkIn < availableFrom || checkOut > availableTo)
		//	{
		//		TempData["Error"] = "Selected dates are outside the availability range.";
		//		return RedirectToAction("Details", "Dar", new { id = HouseID });
		//	}

		//	// Check for overlaps
		//	var overlapping = _context.HouseBookings
		//		.Any(b => b.HouseId == HouseID &&
		//				  ((checkIn >= b.CheckInDate.Value.ToDateTime(TimeOnly.MinValue) && checkIn < b.CheckOutDate.Value.ToDateTime(TimeOnly.MinValue)) ||
		//				   (checkOut > b.CheckInDate.Value.ToDateTime(TimeOnly.MinValue) && checkOut <= b.CheckOutDate.Value.ToDateTime(TimeOnly.MinValue)) ||
		//				   (checkIn <= b.CheckInDate.Value.ToDateTime(TimeOnly.MinValue) && checkOut >= b.CheckOutDate.Value.ToDateTime(TimeOnly.MinValue))));

		//	if (overlapping)
		//	{
		//		TempData["Error"] = "Selected dates are already booked.";
		//		return RedirectToAction("Details", "Dar", new { id = HouseID });
		//	}

		//	// Create booking
		//	var booking = new HouseBooking
		//	{
		//		UserId = userId.Value,
		//		HouseId = HouseID,
		//		CheckInDate = DateOnly.FromDateTime(checkIn),
		//		CheckOutDate = DateOnly.FromDateTime(checkOut),
		//		Guests = Guests,
		//		Status = "Pending",
		//		BookingDate = DateTime.Now
		//	};

		//	_context.HouseBookings.Add(booking);
		//	_context.SaveChanges();

		//	TempData["Success"] = "Your booking was submitted successfully!";
		//	return RedirectToAction("Details", "Dar", new { id = HouseID });
		//}


		[HttpPost]
		public IActionResult BookHouse(int HouseID, string BookingDates, int Guests)
		{
			var userId = HttpContext.Session.GetInt32("UserId");
			if (userId == null)
				return RedirectToAction("Login", "User");

			// Parse Dates
			var dates = BookingDates?.Split(" to ");
			if (dates == null || dates.Length != 2 ||
				!DateTime.TryParse(dates[0], out DateTime checkIn) ||
				!DateTime.TryParse(dates[1], out DateTime checkOut))
			{
				TempData["Error"] = "Invalid booking dates.";
				return RedirectToAction("Details", "Dar", new { id = HouseID });
			}

			// Validate House
			var house = _context.Houses.FirstOrDefault(h => h.Id == HouseID);
			if (house == null || house.Available != true || Guests > house.MaxGuests)
			{
				TempData["Error"] = "House not available or guest count exceeded.";
				return RedirectToAction("Details", "Dar", new { id = HouseID });
			}

			// Validate dates
			var availableFrom = house.AvailableFrom?.ToDateTime(TimeOnly.MinValue);
			var availableTo = house.AvailableTo?.ToDateTime(TimeOnly.MinValue);
			if (checkIn < availableFrom || checkOut > availableTo)
			{
				TempData["Error"] = "Selected dates are outside the house availability.";
				return RedirectToAction("Details", "Dar", new { id = HouseID });
			}

			// Overlapping booking validation
			var existingBookings = _context.HouseBookings
			.Where(b => b.HouseId == HouseID && b.CheckInDate.HasValue && b.CheckOutDate.HasValue)
			.ToList();  // <-- Load to memory

			var overlapping = existingBookings.Any(b =>
			{
				var existingCheckIn = b.CheckInDate.Value.ToDateTime(TimeOnly.MinValue);
				var existingCheckOut = b.CheckOutDate.Value.ToDateTime(TimeOnly.MinValue);

				return (checkIn >= existingCheckIn && checkIn < existingCheckOut) ||
					   (checkOut > existingCheckIn && checkOut <= existingCheckOut) ||
					   (checkIn <= existingCheckIn && checkOut >= existingCheckOut);
			});


			if (overlapping)
			{
				TempData["Error"] = "Selected dates are already booked.";
				return RedirectToAction("Details", "Dar", new { id = HouseID });
			}

			// Create booking
			var booking = new HouseBooking
			{
				UserId = userId.Value,
				HouseId= HouseID,
				CheckInDate = DateOnly.FromDateTime(checkIn),
				CheckOutDate = DateOnly.FromDateTime(checkOut),
				Guests = Guests,
				Status = "Pending",
				BookingDate = DateTime.Now
			};

			_context.HouseBookings.Add(booking);
			_context.SaveChanges();

			// Redirect to the Cart1 page with booking id
			return RedirectToAction("Cart1", "Bookings", new { id = booking.Id });
		}







		public IActionResult Cart1(int id)
		{
			var booking = _context.HouseBookings
				.Include(b => b.House)
				.FirstOrDefault(b => b.Id == id);

			if (booking == null)
				return NotFound();

			// Calculate nights and total price
			var nights = (booking.CheckOutDate.Value.ToDateTime(TimeOnly.MinValue) - booking.CheckInDate.Value.ToDateTime(TimeOnly.MinValue)).Days;
			var total = booking.House.Price * nights;

			ViewBag.Nights = nights;
			ViewBag.TotalPrice = total;

			return View(booking);
		}




        //Payment/////
        public IActionResult Cart2(int id)
        {
            var booking = _context.HouseBookings
                .Include(b => b.House)
                .Include(b => b.User) // Assuming you have a relation to User
                .FirstOrDefault(b => b.Id == id);

            if (booking == null)
                return NotFound();

            var nights = (booking.CheckOutDate.Value.ToDateTime(TimeOnly.MinValue) - booking.CheckInDate.Value.ToDateTime(TimeOnly.MinValue)).Days;
            var total = booking.House.Price * nights;

            ViewBag.Nights = nights;
            ViewBag.TotalPrice = total;

            return View(booking);
        }

        [HttpPost]
        public async Task<IActionResult> Purchase(int bookingId, string paymentMethod, string CardName, string CardNumber, string ExpireMonth, string ExpireYear, string Cvv, string Country, string Street1, string City, string State)
        {
            var booking = _context.HouseBookings
                .Include(b => b.House)
                .Include(b => b.User)
                .FirstOrDefault(b => b.Id == bookingId);

            if (booking == null)
                return NotFound();

            // Validate input
            if (string.IsNullOrWhiteSpace(paymentMethod) ||
                string.IsNullOrWhiteSpace(CardName) ||
                string.IsNullOrWhiteSpace(CardNumber) ||
                string.IsNullOrWhiteSpace(ExpireMonth) ||
                string.IsNullOrWhiteSpace(ExpireYear) ||
                string.IsNullOrWhiteSpace(Cvv) ||
                string.IsNullOrWhiteSpace(Country) ||
                string.IsNullOrWhiteSpace(Street1) ||
                string.IsNullOrWhiteSpace(City) ||
                string.IsNullOrWhiteSpace(State))
            {
                TempData["Error"] = "Please fill in all fields.";
                return RedirectToAction("Cart2", new { id = bookingId });
            }

            if (CardNumber.Length < 12 || CardNumber.Length > 19)
            {
                TempData["Error"] = "Invalid Card Number.";
                return RedirectToAction("Cart2", new { id = bookingId });
            }

            if (Cvv.Length != 3)
            {
                TempData["Error"] = "Invalid Security Code.";
                return RedirectToAction("Cart2", new { id = bookingId });
            }

            var nights = (booking.CheckOutDate.Value.ToDateTime(TimeOnly.MinValue) - booking.CheckInDate.Value.ToDateTime(TimeOnly.MinValue)).Days;
            var total = booking.House.Price * nights;

            // Save payment
            var payment = new Payment
            {
                BookingId = bookingId,
                UserId = booking.UserId,
                Amount = total,
                PaymentMethod = paymentMethod,
                Status = "Completed",
                PaymentDate = DateTime.Now
            };

            _context.Payments.Add(payment);
            // ✅ Update the booking status to Confirmed
            booking.Status = "Confirmed";
            await _context.SaveChangesAsync();

            // Send confirmation email
            await SendConfirmationEmail(booking.User.Email, booking, payment);

            TempData["UserEmail"] = booking.User.Email;
            TempData["Success"] = "Your booking has been confirmed!";
            TempData["Reference"] = $"DN-{DateTime.Now:yyyyMMddHHmmss}"; // Optional Reference Number

            TempData["Success"] = "Payment Completed Successfully!";
            return RedirectToAction("Cart3");
        }


        private async Task SendConfirmationEmail(string toEmail, HouseBooking booking, Payment payment)
        {
            try
            {
                var smtpClient = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential("hanijarrah20@gmail.com", "bfjy ycst hihg mlin"),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("hanijarrah20@gmail.com", "DarNA Team"),
                    Subject = "Booking Confirmation - DarNA",
                    IsBodyHtml = true,
                    Body = $@"<div style='font-family: Arial, sans-serif; max-width:600px; margin:auto; border:1px solid #ddd; padding:20px;'>
                        <div style='text-align:center; margin-bottom:20px;'>
                            <img src='~/Images/img/DarNa دارنا.png' alt='DarNA Logo' style='width:150px; height:auto;'>
                        </div>
                        <h2 style='color: #2E86C1; text-align:center;'>Booking Confirmation</h2>
                        <p style='text-align:center;'>Dear <strong>{booking.User.FullName} </strong>,</p>
                        <p style='text-align:center;'>Thank you for booking <strong>{booking.House.Name}</strong> with DarNA!</p>
                        <h4>Booking Details:</h4>
                        <ul>
                            <li><strong>Check-in:</strong> {booking.CheckInDate}</li>
                            <li><strong>Check-out:</strong> {booking.CheckOutDate}</li>
                            <li><strong>Guests:</strong> {booking.Guests}</li>
                            <li><strong>Total Amount:</strong> {payment.Amount} J.D</li>
                            <li><strong>Payment Method:</strong> {payment.PaymentMethod}</li>
                        </ul>
                        <hr>
                        <p style='text-align:center; color: #888;'>Best regards,<br>DarNA Team | Jordan</p>
                      </div>"
                };

                mailMessage.To.Add(toEmail);

                // Now generate PDF invoice
                using (var ms = new MemoryStream())
                {
                    var document = new PdfSharpCore.Pdf.PdfDocument();
                    var page = document.AddPage();
                    var gfx = PdfSharpCore.Drawing.XGraphics.FromPdfPage(page);
                    var font = new PdfSharpCore.Drawing.XFont("Arial", 14, PdfSharpCore.Drawing.XFontStyle.Regular);

                    gfx.DrawString("DarNA Booking Invoice", font, PdfSharpCore.Drawing.XBrushes.Black, new PdfSharpCore.Drawing.XRect(0, 0, page.Width, page.Height), PdfSharpCore.Drawing.XStringFormats.TopCenter);

                    gfx.DrawString($"Name: {booking.User.FullName} ", font, PdfSharpCore.Drawing.XBrushes.Black, new PdfSharpCore.Drawing.XPoint(40, 80));
                    gfx.DrawString($"House: {booking.House.Name}", font, PdfSharpCore.Drawing.XBrushes.Black, new PdfSharpCore.Drawing.XPoint(40, 110));
                    gfx.DrawString($"Check-in: {booking.CheckInDate}", font, PdfSharpCore.Drawing.XBrushes.Black, new PdfSharpCore.Drawing.XPoint(40, 140));
                    gfx.DrawString($"Check-out: {booking.CheckOutDate}", font, PdfSharpCore.Drawing.XBrushes.Black, new PdfSharpCore.Drawing.XPoint(40, 170));
                    gfx.DrawString($"Guests: {booking.Guests}", font, PdfSharpCore.Drawing.XBrushes.Black, new PdfSharpCore.Drawing.XPoint(40, 200));
                    gfx.DrawString($"Total Amount: {payment.Amount} J.D", font, PdfSharpCore.Drawing.XBrushes.Black, new PdfSharpCore.Drawing.XPoint(40, 230));
                    gfx.DrawString($"Payment Method: {payment.PaymentMethod}", font, PdfSharpCore.Drawing.XBrushes.Black, new PdfSharpCore.Drawing.XPoint(40, 260));

                    document.Save(ms, false);

                    ms.Seek(0, SeekOrigin.Begin);

                    var attachment = new Attachment(ms, "DarNA_Booking_Invoice.pdf", "application/pdf");
                    mailMessage.Attachments.Add(attachment);

                    await smtpClient.SendMailAsync(mailMessage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email: {ex.Message}");
            }
        }


        public IActionResult Cart3()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CancelHouseBooking(int id)
        {
            var booking = _context.HouseBookings.FirstOrDefault(b => b.Id == id);

            if (booking == null || (booking.Status != "Pending" && booking.Status != "Confirmed"))
            {
                TempData["Error"] = "Invalid or non-cancelable booking.";
                return RedirectToAction("Profile", "User");
            }

            var todayPlus2 = DateOnly.FromDateTime(DateTime.Today.AddDays(2));
            if (booking.CheckInDate <= todayPlus2)
            {
                TempData["Error"] = "Cancellations must be made at least 2 days before the check-in date.";
                return RedirectToAction("Profile", "User");
            }

            // ✅ Update status instead of deleting (to avoid FK constraint violation)
            booking.Status = "Cancelled";
            _context.SaveChanges();

            TempData["Success"] = "House booking canceled successfully.";
            return RedirectToAction("Profile", "User");
        }

        //////////TOUR BOOKING///////

        [HttpPost]
        public IActionResult BookTour(int tourId, int guests)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "User");

            var tour = _context.Tours.FirstOrDefault(t => t.Id == tourId);
            if (tour == null) return NotFound();

            int existingBookings = (int)_context.TourBookings
                .Where(b => b.TourId == tourId && b.Status != "Cancelled")
                .Sum(b => b.Guests);

            if (existingBookings + guests > tour.MaxGuests)
            {
                TempData["Error"] = "Not enough available spots.";
                return RedirectToAction("TourDetails", "Home", new { id = tourId });
            }

            var booking = new TourBooking
            {
                TourId = tourId,
                UserId = userId.Value,
                Guests = guests,
                Status = "Pending",
                BookingDate = DateTime.Now
            };

            _context.TourBookings.Add(booking);
            _context.SaveChanges();

            return RedirectToAction("TourCart1", new { bookingId = booking.Id });
        }
        public IActionResult TourCart1(int bookingId)
        {
            var booking = _context.TourBookings
                .Include(b => b.Tour)
                .FirstOrDefault(b => b.Id == bookingId);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking); // This loads TourCart1.cshtml
        }

        // TourCart2 (GET)
        public IActionResult TourCart2(int id)
        {
            var booking = _context.TourBookings
                .Include(b => b.Tour)
                .Include(b => b.User)
                .FirstOrDefault(b => b.Id == id);

            if (booking == null)
                return NotFound();

            var total = booking.Tour.Price * booking.Guests;
            ViewBag.TotalPrice = total;

            return View(booking);
        }

        // PurchaseTour (POST)
        [HttpPost]
        public async Task<IActionResult> PurchaseTour(int bookingId, string paymentMethod, string CardName, string CardNumber, string ExpireMonth, string ExpireYear, string Cvv, string Country, string Street1, string City, string State)
        {
            var booking = _context.TourBookings
                .Include(b => b.Tour)
                .Include(b => b.User)
                .FirstOrDefault(b => b.Id == bookingId);

            if (booking == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(paymentMethod) ||
                string.IsNullOrWhiteSpace(CardName) ||
                string.IsNullOrWhiteSpace(CardNumber) ||
                string.IsNullOrWhiteSpace(ExpireMonth) ||
                string.IsNullOrWhiteSpace(ExpireYear) ||
                string.IsNullOrWhiteSpace(Cvv) ||
                string.IsNullOrWhiteSpace(Country) ||
                string.IsNullOrWhiteSpace(Street1) ||
                string.IsNullOrWhiteSpace(City) ||
                string.IsNullOrWhiteSpace(State))
            {
                TempData["Error"] = "Please fill in all fields.";
                return RedirectToAction("TourCart2", new { id = bookingId });
            }

            if (CardNumber.Length < 12 || CardNumber.Length > 19 || Cvv.Length != 3)
            {
                TempData["Error"] = "Invalid card details.";
                return RedirectToAction("TourCart2", new { id = bookingId });
            }

            var total = booking.Tour.Price * booking.Guests;

            var payment = new Payment
            {
                BookingId = bookingId,
                UserId = booking.UserId,
                Amount = total,
                PaymentMethod = paymentMethod,
                Status = "Completed",
                PaymentDate = DateTime.Now
            };

            _context.Payments.Add(payment);
            booking.Status = "Confirmed";
            await _context.SaveChangesAsync();

            await SendTourConfirmationEmail(booking.User.Email, booking, payment);

            TempData["UserEmail"] = booking.User.Email;
            TempData["Reference"] = $"TOUR-{DateTime.Now:yyyyMMddHHmmss}";
            TempData["Success"] = "Tour payment completed successfully!";

            return RedirectToAction("TourCart3");
        }

        private async Task SendTourConfirmationEmail(string toEmail, TourBooking booking, Payment payment)
        {
            try
            {
                var smtpClient = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential("hanijarrah20@gmail.com", "bfjy ycst hihg mlin"),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("hanijarrah20@gmail.com", "DarNA Team"),
                    Subject = "Tour Booking Confirmation - DarNA",
                    IsBodyHtml = true,
                    Body = $@"<div style='font-family: Arial; padding: 20px;'>
                <h2 style='color:#2E86C1;'>Tour Booking Confirmation</h2>
                <p>Dear <strong>{booking.User.FullName}</strong>,</p>
                <p>Thank you for booking <strong>{booking.Tour.Name}</strong>!</p>
                <ul>
                    <li><strong>Date:</strong> {booking.Tour.StartDate:yyyy-MM-dd}</li>
                    <li><strong>Guests:</strong> {booking.Guests}</li>
                    <li><strong>Total:</strong> {payment.Amount} J.D</li>
                    <li><strong>Method:</strong> {payment.PaymentMethod}</li>
                </ul>
                <p>You will find the invoice attached.</p>
                <p style='color:#888;'>Best regards,<br/>DarNA Team</p>
            </div>"
                };

                mailMessage.To.Add(toEmail);

                using (var ms = new MemoryStream())
                {
                    var doc = new PdfSharpCore.Pdf.PdfDocument();
                    var page = doc.AddPage();
                    var gfx = PdfSharpCore.Drawing.XGraphics.FromPdfPage(page);
                    var font = new PdfSharpCore.Drawing.XFont("Arial", 14);

                    gfx.DrawString("Tour Booking Invoice", font, PdfSharpCore.Drawing.XBrushes.Black, new PdfSharpCore.Drawing.XRect(0, 0, page.Width, page.Height), PdfSharpCore.Drawing.XStringFormats.TopCenter);
                    gfx.DrawString($"Name: {booking.User.FullName}", font, PdfSharpCore.Drawing.XBrushes.Black, 40, 80);
                    gfx.DrawString($"Tour: {booking.Tour.Name}", font, PdfSharpCore.Drawing.XBrushes.Black, 40, 110);
                    gfx.DrawString($"Date: {booking.Tour.StartDate:yyyy-MM-dd}", font, PdfSharpCore.Drawing.XBrushes.Black, 40, 140);
                    gfx.DrawString($"Guests: {booking.Guests}", font, PdfSharpCore.Drawing.XBrushes.Black, 40, 170);
                    gfx.DrawString($"Total: {payment.Amount} J.D", font, PdfSharpCore.Drawing.XBrushes.Black, 40, 200);
                    gfx.DrawString($"Method: {payment.PaymentMethod}", font, PdfSharpCore.Drawing.XBrushes.Black, 40, 230);

                    doc.Save(ms, false);
                    ms.Seek(0, SeekOrigin.Begin);

                    mailMessage.Attachments.Add(new Attachment(ms, "TourBookingInvoice.pdf", "application/pdf"));
                    await smtpClient.SendMailAsync(mailMessage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email error: {ex.Message}");
            }
        }





        public IActionResult TourCart3()
        {
            return View();
        }
    }
}
