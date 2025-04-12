using MasterPiece.Models;
using Microsoft.AspNetCore.Mvc;
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

        public IActionResult Index(
    string? terms,
    string? location,
    List<int>? stars,
    DateTime? availableFrom,
    int? minPrice,
    int? maxPrice,
    int? guests,
    string? sortBy
)
        {
            var houses = _context.Houses.AsQueryable();

            if (!string.IsNullOrEmpty(terms))
                houses = houses.Where(h => h.Name.Contains(terms) || h.Description.Contains(terms));

            if (!string.IsNullOrEmpty(location))
                houses = houses.Where(h => h.LocationName == location);

            if (stars?.Any() == true)
                houses = houses.Where(h => h.Stars.HasValue && stars.Contains(h.Stars.Value));

            //if (availableFrom.HasValue)
            //{
            //    DateTime dateOnly = availableFrom.Value.Date;
            //    houses = houses.Where(h => h.AvailableFrom.HasValue && h.AvailableFrom.Value.ToString <= dateOnly);
            //}


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

    }
}
