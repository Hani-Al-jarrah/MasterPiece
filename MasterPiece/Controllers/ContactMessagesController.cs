using MasterPiece.Models;
using Microsoft.AspNetCore.Mvc;

namespace MasterPiece.Controllers
{
    public class ContactMessagesController : Controller
    {
        private readonly MyDbContext _context;

        public ContactMessagesController(MyDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var messages = _context.ContactUs.OrderByDescending(m => m.CreatedAt).ToList();
            return View(messages);
        }
    }
}
