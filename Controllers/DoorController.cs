using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartDoors.Data;
using SmartDoors.Filters;
using SmartDoors.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SmartDoors.Controllers
{
    [SessionAuthorize]
    public class DoorController : Controller
    {
        private readonly SmartDoorContext _context;

        public DoorController(SmartDoorContext context)
        {
            _context = context;
        }

        // Kapı listesi ve bağlı cihazları göster
        public async Task<IActionResult> Index(string? searchName)
        {
            var query = _context.Doors
                .Include(d => d.UserDoors)
                    .ThenInclude(ud => ud.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchName))
            {
                query = query.Where(d => d.DoorName!.Contains(searchName));
            }

            var doors = await query.ToListAsync();
            ViewBag.SearchName = searchName;

            return View(doors);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }


        // Yeni kapı kaydet
        [HttpPost]
        public async Task<IActionResult> Create(Door door)
        {
            if (!ModelState.IsValid)
                return View(door);

            _context.Doors.Add(door);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Details(int id)
        {
            var door = await _context.Doors
                .Include(d => d.UserDoors)
                    .ThenInclude(ud => ud.User)
                .FirstOrDefaultAsync(d => d.DoorID == id);

            if (door == null) return NotFound();

            return View(door);
        }

        // Kapı düzenleme formu
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var door = await _context.Doors.FindAsync(id);
            if (door == null) return NotFound();
            return View(door);
        }

        // Kapı düzenle kaydet
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Door door)
        {
            if (id != door.DoorID)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(door);

            try
            {
                _context.Update(door);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Doors.Any(d => d.DoorID == id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        
    }
}

