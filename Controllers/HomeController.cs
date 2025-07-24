using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartDoors.Data;
using SmartDoors.Filters;
using SmartDoors.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SmartDoors.Controllers
{
    [SessionAuthorize]
    public class HomeController : Controller
    {
        private readonly SmartDoorContext _context;

        public HomeController(SmartDoorContext context)
        {
            _context = context;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // İstek: tarih aralığı ve hata filtresi eklendi
        public async Task<IActionResult> Index(
            string sortOrder = "date_desc",
            string searchString = "",
            int? doorId = null,
            string startDate = null,
            string endDate = null,
            string errorStatus = null)
        {
            var logsQuery = _context.Logs
                .Include(l => l.User)
                    .ThenInclude(u => u.UserDoors)
                .Include(l => l.Door)
                .AsQueryable();

            // İsim veya soyisim filtrelemesi
            if (!string.IsNullOrEmpty(searchString))
            {
                logsQuery = logsQuery.Where(l =>
                    l.User.FirstName.Contains(searchString) || l.User.LastName.Contains(searchString));
            }

            // Kapı filtresi
            if (doorId.HasValue)
            {
                logsQuery = logsQuery.Where(l => l.DoorID == doorId.Value);
            }

            // Tarih aralığı filtrelemesi
            if (DateTime.TryParse(startDate, out DateTime start))
            {
                logsQuery = logsQuery.Where(l => l.Timestamp >= start);
            }

            if (DateTime.TryParse(endDate, out DateTime end))
            {
                logsQuery = logsQuery.Where(l => l.Timestamp < end.AddDays(1)); // bitiş tarihini dahil etmek için
            }

            // Hata durumu filtrelemesi
            if (!string.IsNullOrEmpty(errorStatus))
            {
                if (bool.TryParse(errorStatus, out bool errorBool))
                {
                    logsQuery = logsQuery.Where(l => l.ErrorStatus == errorBool);
                }
            }

            // Sıralama
            logsQuery = sortOrder switch
            {
                "date_asc" => logsQuery.OrderBy(l => l.Timestamp),
                "date_desc" => logsQuery.OrderByDescending(l => l.Timestamp),
                _ => logsQuery.OrderByDescending(l => l.Timestamp),
            };

            var logs = await logsQuery.ToListAsync();

            var logViewModels = logs.Select(l => new LogViewModel
            {
                LogID = l.LogID,
                UserFullName = $"{l.User.FirstName} {l.User.LastName}",
                DoorName = l.Door.DoorName,
                Timestamp = l.Timestamp,
                ErrorStatus = l.ErrorStatus,
                IsEntry = l.Operation,
                HasAccess = l.User.UserDoors.Any(ud => ud.DoorID == l.DoorID && ud.AccessGranted)
            }).ToList();

            ViewBag.Doors = await _context.Doors.ToListAsync();
            ViewBag.SelectedDoorId = doorId?.ToString() ?? "";
            ViewBag.SearchString = searchString;
            ViewBag.SortOrder = sortOrder;
            ViewBag.StartDate = startDate ?? "";
            ViewBag.EndDate = endDate ?? "";
            ViewBag.ErrorStatus = errorStatus ?? "";

            return View(logViewModels);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUserAccess(int userId, int doorId, bool accessGranted)
        {
            var userDoor = await _context.UserDoors
                .FirstOrDefaultAsync(ud => ud.UserID == userId && ud.DoorID == doorId);

            if (userDoor == null)
            {
                userDoor = new Models.UserDoor
                {
                    UserID = userId,
                    DoorID = doorId,
                    AccessGranted = accessGranted
                };
                _context.UserDoors.Add(userDoor);
            }
            else
            {
                userDoor.AccessGranted = accessGranted;
                _context.UserDoors.Update(userDoor);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
