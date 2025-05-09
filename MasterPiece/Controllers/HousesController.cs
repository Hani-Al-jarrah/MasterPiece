using MasterPiece.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace MasterPiece.Controllers
{
    public class HousesController : Controller
    {
        private readonly MyDbContext _context;

        public HousesController(MyDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var houses = _context.Houses.ToList();

            return View(houses);
        }
     
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(House house, IFormFile ImageFile)
        {

            if (ImageFile != null && ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "newImages");
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + ImageFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                house.ImageUrl = "/newImages/" + uniqueFileName; // 👈 Save relative path
            }

            house.Available = true; // default
            _context.Houses.Add(house);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");

        }
        public IActionResult ShowImages(int id)
        {
            var images = _context.Images.FirstOrDefault(i => i.HouseId == id);
            if (images == null)
            {
                TempData["Message"] = "No images found for this house.";
                return RedirectToAction("Index");
            }

            ViewBag.HouseId = id;
            return View(images);
        }





        public IActionResult Edit(int id)
        {
            var house = _context.Houses.FirstOrDefault(h => h.Id == id);
            if (house == null)
                return NotFound();

            return View(house);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(House house, IFormFile ImageFile)
        {

            var existingHouse = await _context.Houses.FindAsync(house.Id);
            if (existingHouse == null)
                return NotFound();

            // Update properties
            existingHouse.Name = house.Name;
            existingHouse.Description = house.Description;
            existingHouse.LocationName = house.LocationName;
            existingHouse.TypeName = house.TypeName;
            existingHouse.Price = house.Price;
            existingHouse.Available = house.Available;
            existingHouse.MaxGuests = house.MaxGuests;
            existingHouse.AvailableFrom = house.AvailableFrom;
            existingHouse.AvailableTo = house.AvailableTo;
            existingHouse.Stars = house.Stars;
            existingHouse.Rate = house.Rate;

            // Upload new image if provided
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "newImages");
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + ImageFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                existingHouse.ImageUrl = "/newImages/" + uniqueFileName;
            }

            _context.Houses.Update(existingHouse);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");

        }

        public IActionResult EditImages(int id)
        {
            var images = _context.Images.FirstOrDefault(i => i.HouseId == id);

            if (images == null)
            {
                // If no record exists, create a blank one and redirect to EditImages again
                var newImages = new Image { HouseId = id };
                _context.Images.Add(newImages);
                _context.SaveChanges();

                return RedirectToAction("EditImages", new { id = id });
            }

            return View(images);
        }

        [HttpPost]
        public async Task<IActionResult> EditImages(int id, IFormFile Image1, IFormFile Image2, IFormFile Image3)
        {
            var images = _context.Images.FirstOrDefault(i => i.HouseId == id);
            if (images == null) return NotFound(); // Shouldn't happen, but just in case

            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "newImages");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            if (Image1 != null && Image1.Length > 0)
            {
                var path1 = Path.Combine(folder, Guid.NewGuid() + "_" + Image1.FileName);
                using (var stream = new FileStream(path1, FileMode.Create))
                {
                    await Image1.CopyToAsync(stream);
                }
                images.ImageUrl1 = "/newImages/" + Path.GetFileName(path1);
            }

            if (Image2 != null && Image2.Length > 0)
            {
                var path2 = Path.Combine(folder, Guid.NewGuid() + "_" + Image2.FileName);
                using (var stream = new FileStream(path2, FileMode.Create))
                {
                    await Image2.CopyToAsync(stream);
                }
                images.ImageUrl2 = "/newImages/" + Path.GetFileName(path2);
            }

            if (Image3 != null && Image3.Length > 0)
            {
                var path3 = Path.Combine(folder, Guid.NewGuid() + "_" + Image3.FileName);
                using (var stream = new FileStream(path3, FileMode.Create))
                {
                    await Image3.CopyToAsync(stream);
                }
                images.ImageUrl3 = "/newImages/" + Path.GetFileName(path3);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }




    }
}
