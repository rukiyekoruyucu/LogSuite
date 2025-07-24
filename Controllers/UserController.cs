using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartDoors.Data;
using SmartDoors.Filters;
using SmartDoors.Models;
using SmartDoors.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace SmartDoors.Controllers
{
    [SessionAuthorize]
    public class UserController : Controller
    {
        private readonly SmartDoorContext _context;

        public UserController(SmartDoorContext context)
        {
            _context = context;
        }

        // Listele - Tüm kullanıcılar
        public async Task<IActionResult> Index(string searchName = "", string cardId = "", string status = "all")
        {
            var usersQuery = _context.Users
                .Include(u => u.UserDoors)
                    .ThenInclude(ud => ud.Door)
                .AsQueryable();

            // İsme göre filtreleme
            if (!string.IsNullOrWhiteSpace(searchName))
            {
                usersQuery = usersQuery.Where(u =>
                    u.FirstName.Contains(searchName) || u.LastName.Contains(searchName));
            }

            // CardID'ye göre filtreleme
            if (!string.IsNullOrWhiteSpace(cardId))
            {
                usersQuery = usersQuery.Where(u => u.CardID.Contains(cardId));
            }

            // Aktiflik durumuna göre filtreleme
            if (status == "active")
            {
                usersQuery = usersQuery.Where(u => u.IsActive);
            }
            else if (status == "inactive")
            {
                usersQuery = usersQuery.Where(u => !u.IsActive);
            }

            var users = await usersQuery.ToListAsync();

            // Filtre değerlerini view'e geri gönderiyoruz
            ViewBag.SearchName = searchName;
            ViewBag.CardId = cardId;
            ViewBag.Status = status;

            return View(users);
        }


        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public IActionResult Create()
        {
            var vm = new UserFormViewModel
            {
                AllDoors = _context.Doors.Select(d => new SelectListItem
                {
                    Value = d.DoorID.ToString(),
                    Text = d.DoorName
                }).ToList(),
                SelectedDoorIds = new List<int>()
            };

            return View(vm); // Views/User/Create.cshtml olmalı
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.AllDoors = _context.Doors.Select(d => new SelectListItem
                {
                    Value = d.DoorID.ToString(),
                    Text = d.DoorName
                }).ToList();
                return View(vm);
            }

            var normalizedCardId = vm.CardID?.Trim().ToUpper();

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.CardID != null && u.CardID.ToUpper() == normalizedCardId);

            if (existingUser != null)
            {
                ModelState.AddModelError(string.Empty, "Bu kart ID zaten başka bir kullanıcıya ait.");
                vm.AllDoors = _context.Doors.Select(d => new SelectListItem
                {
                    Value = d.DoorID.ToString(),
                    Text = d.DoorName
                }).ToList();
                return View(vm);
            }

            var user = new User
            {
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                CardID = vm.CardID.Trim(),
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            foreach (var doorId in vm.SelectedDoorIds)
            {
                _context.UserDoors.Add(new UserDoor
                {
                    UserID = user.UserID,
                    DoorID = doorId,
                    AccessGranted = true
                });
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: Kullanıcı düzenle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserFormViewModel vm)
        {
            if (id != vm.UserID) return NotFound();

            if (!ModelState.IsValid)
            {
                vm.AllDoors = _context.Doors.Select(d => new SelectListItem
                {
                    Value = d.DoorID.ToString(),
                    Text = d.DoorName
                }).ToList();
                return View(vm);
            }

            var user = await _context.Users
                .Include(u => u.UserDoors)
                .FirstOrDefaultAsync(u => u.UserID == id);

            if (user == null) return NotFound();

            user.FirstName = vm.FirstName;
            user.LastName = vm.LastName;
            user.CardID = vm.CardID;

            _context.UserDoors.RemoveRange(user.UserDoors);

            foreach (var doorId in vm.SelectedDoorIds)
            {
                _context.UserDoors.Add(new UserDoor
                {
                    UserID = user.UserID,
                    DoorID = doorId,
                    AccessGranted = true
                });
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        // GET: Kullanıcı düzenleme formunu göster
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Users
                .Include(u => u.UserDoors)
                .FirstOrDefaultAsync(u => u.UserID == id);

            if (user == null) return NotFound();

            var vm = new UserFormViewModel
            {
                UserID = user.UserID,
                FirstName = user.FirstName,
                LastName = user.LastName,
                CardID = user.CardID,
                SelectedDoorIds = user.UserDoors.Select(ud => ud.DoorID).ToList(),
                AllDoors = _context.Doors.Select(d => new SelectListItem
                {
                    Value = d.DoorID.ToString(),
                    Text = d.DoorName
                }).ToList()
            };

            return View(vm);
        }
        // GET: Kullanıcı silme onay sayfası
/*[HttpGet]
public async Task<IActionResult> Delete(int id)
{
    var user = await _context.Users
        .Include(u => u.UserDoors)
        .FirstOrDefaultAsync(u => u.UserID == id);

    if (user == null) return NotFound();

    return View(user);
}



// POST: Kullanıcı silme onaylandı
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> DeleteConfirmed(int id)
{
    var user = await _context.Users
        .Include(u => u.UserDoors)
        .FirstOrDefaultAsync(u => u.UserID == id);

    if (user != null)
    {
        // İlişkili UserDoors kayıtlarını sil
        _context.UserDoors.RemoveRange(user.UserDoors);

        // İlişkili Log kayıtlarını sil
        var logs = _context.Logs.Where(l => l.UserID == id);
        _context.Logs.RemoveRange(logs);

        // Kullanıcıyı sil
        _context.Users.Remove(user);

        await _context.SaveChangesAsync();
    }

    return RedirectToAction(nameof(Index));
}*/

}
}
