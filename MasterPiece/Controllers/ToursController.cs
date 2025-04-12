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
    }
}
