using MasterPiece.Models;
using Microsoft.AspNetCore.Mvc;

namespace MasterPiece.Controllers
{
    public class ToursController : Controller
    {
        private readonly MyDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ToursController(MyDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public IActionResult Index()
        {
            var tours = _context.Tours.OrderByDescending(t => t.CreatedAt).ToList();
            return View(tours);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Tour tour, IFormFile ImageFile)
        {
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var folder = Path.Combine(_env.WebRootPath, "newImages");
                var fileName = Guid.NewGuid().ToString() + "_" + ImageFile.FileName;
                var path = Path.Combine(folder, fileName);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                tour.ImageUrl = "/newImages/" + fileName;
            }

            tour.CreatedAt = DateTime.Now;
            _context.Tours.Add(tour);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");

        }



        public IActionResult Edit(int id)
        {
            var tour = _context.Tours.FirstOrDefault(t => t.Id == id);
            if (tour == null) return NotFound();
            return View(tour);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Tour tour, IFormFile ImageFile)
        {

            var existingTour = _context.Tours.FirstOrDefault(t => t.Id == tour.Id);
            if (existingTour == null) return NotFound();

            existingTour.Name = tour.Name;
            existingTour.Description = tour.Description;
            existingTour.LocationName = tour.LocationName;
            existingTour.Duration = tour.Duration;
            existingTour.Price = tour.Price;
            existingTour.MaxGuests = tour.MaxGuests;
            existingTour.StartDate = tour.StartDate;
            existingTour.StartTime = tour.StartTime;
            existingTour.EndDate = tour.EndDate;
            existingTour.EndTime = tour.EndTime;

            if (ImageFile != null && ImageFile.Length > 0)
            {
                var folder = Path.Combine(_env.WebRootPath, "newImages");
                var fileName = Guid.NewGuid().ToString() + "_" + ImageFile.FileName;
                var path = Path.Combine(folder, fileName);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                existingTour.ImageUrl = "/newImages/" + fileName;
            }

            _context.Tours.Update(existingTour);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");

        }

        public IActionResult EditImages(int id)
        {
            var images = _context.Images.FirstOrDefault(i => i.TourId == id);

            if (images == null)
            {
                var newImages = new Image { TourId = id };
                _context.Images.Add(newImages);
                _context.SaveChanges();

                return RedirectToAction("EditImages", new { id });
            }

            return View(images);
        }

        [HttpPost]
        public async Task<IActionResult> EditImages(int id, IFormFile Image1, IFormFile Image2, IFormFile Image3)
        {
            var images = _context.Images.FirstOrDefault(i => i.TourId == id);
            if (images == null) return NotFound();

            var folder = Path.Combine(_env.WebRootPath, "newImages");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            if (Image1 != null && Image1.Length > 0)
            {
                var path1 = Path.Combine(folder, Guid.NewGuid() + "_" + Image1.FileName);
                using (var stream = new FileStream(path1, FileMode.Create))
                    await Image1.CopyToAsync(stream);
                images.ImageUrl1 = "/newImages/" + Path.GetFileName(path1);
            }

            if (Image2 != null && Image2.Length > 0)
            {
                var path2 = Path.Combine(folder, Guid.NewGuid() + "_" + Image2.FileName);
                using (var stream = new FileStream(path2, FileMode.Create))
                    await Image2.CopyToAsync(stream);
                images.ImageUrl2 = "/newImages/" + Path.GetFileName(path2);
            }

            if (Image3 != null && Image3.Length > 0)
            {
                var path3 = Path.Combine(folder, Guid.NewGuid() + "_" + Image3.FileName);
                using (var stream = new FileStream(path3, FileMode.Create))
                    await Image3.CopyToAsync(stream);
                images.ImageUrl3 = "/newImages/" + Path.GetFileName(path3);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public IActionResult ShowImages(int id)
        {
            var images = _context.Images.FirstOrDefault(i => i.TourId == id);
            if (images == null)
            {
                TempData["Message"] = "No images found for this tour.";
                return RedirectToAction("Index");
            }

            ViewBag.TourId = id;
            return View(images);
        }


    }
}






