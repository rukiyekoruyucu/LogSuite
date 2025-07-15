using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartDoors.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SmartDoors.Controllers
{
    public class HomeController : Controller
    {
        private readonly SmartDoorContext _context;

        public HomeController(SmartDoorContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string sortOrder, string searchString)
        {
            var logs = _context.Logs.Include(l => l.User).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                logs = logs.Where(l =>
                    l.User.FirstName.Contains(searchString) ||
                    l.User.LastName.Contains(searchString));
            }

            logs = sortOrder switch
            {
                "date_desc" => logs.OrderByDescending(l => l.Timestamp),
                "date_asc" => logs.OrderBy(l => l.Timestamp),
                "error" => logs.Where(l => l.ErrorStatus),
                _ => logs.OrderByDescending(l => l.Timestamp)
            };

            // 👇 ViewBag ekle
            ViewBag.SearchString = searchString;
            ViewBag.SortOrder = sortOrder;

            return View(await logs.ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUserAccess(int userId, bool doorAccess)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.DoorAccess = doorAccess;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
