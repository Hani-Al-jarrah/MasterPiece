using MasterPiece.Models;
using Microsoft.AspNetCore.Mvc;

namespace MasterPiece.Controllers
{
    public class UsersController : Controller
    {
        private readonly MyDbContext _context;

        public UsersController(MyDbContext context)
        {
            _context = context;
        }
        // IN THE ADMIN SIDE FOR SEEING THE USERS AND DELETE THEM IF NEED 
        public IActionResult Index()
        {
            var users = _context.Users.ToList();
            return View(users);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                return NotFound();

            _context.Users.Remove(user);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
