using MasterPiece.Models;
using Microsoft.AspNetCore.Mvc;
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

    }
}
