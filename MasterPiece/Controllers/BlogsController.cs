using MasterPiece.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MasterPiece.Controllers
{
    public class BlogsController : Controller
    {
        private readonly MyDbContext _context;

        public BlogsController(MyDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var blogs = _context.Blogs
                .Include(b => b.Author)
                .OrderByDescending(b => b.CreatedAt)
                .ToList();

            return View(blogs);
        }
    }
}
