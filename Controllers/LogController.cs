using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartDoors.Data;
using SmartDoors.Filters;
using SmartDoors.Models;
using SmartDoors.ViewModels;
using System;
using System.Linq;

namespace SmartDoors.Controllers
{
    [SessionAuthorize]
    public class LogController : Controller
    {
        private readonly SmartDoorContext _context;

        public LogController(SmartDoorContext context)
        {
            _context = context;
        }

        // GET: Log/Add
        [HttpGet]
        public IActionResult Add()
        {
            var model = new AddLogViewModel
            {
                Doors = _context.Doors.Select(d => new SelectListItem
                {
                    Value = d.DoorName,
                    Text = d.DoorName
                }).ToList()
            };

            return View(model);
        }

        // POST: Log/Add
        [HttpPost]
        public IActionResult Add(AddLogViewModel model)
        {
            // Dropdown tekrar yüklensin
            model.Doors = _context.Doors.Select(d => new SelectListItem
            {
                Value = d.DoorName,
                Text = d.DoorName
            }).ToList();

            if (!ModelState.IsValid)
                return View(model);

            if (model.IsEntry && model.IsExit)
            {
                ModelState.AddModelError("", "Giriş ve Çıkış aynı anda seçilemez.");
                return View(model);
            }

            if (!model.IsEntry && !model.IsExit)
            {
                ModelState.AddModelError("", "Lütfen Giriş veya Çıkış seçiniz.");
                return View(model);
            }

            var user = _context.Users.FirstOrDefault(u => u.CardID == model.CardID);
            if (user == null)
            {
                ModelState.AddModelError("CardID", "Bu karta ait kullanıcı bulunamadı.");
                return View(model);
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError("CardID", "Bu kullanıcı pasif durumdadır. Kayıt yapılamaz.");
                return View(model);
            }

            var door = _context.Doors.FirstOrDefault(d => d.DoorName == model.DoorName);
            if (door == null)
            {
                ModelState.AddModelError("DoorName", "Seçilen kapı sistemde kayıtlı değil.");
                return View(model);
            }

            var isAuthorized = _context.UserDoors.Any(ud => ud.UserID == user.UserID && ud.DoorID == door.DoorID && ud.AccessGranted);
            if (!isAuthorized)
            {
                ModelState.AddModelError("DoorName", "Kullanıcının bu kapıya erişim yetkisi yok.");
                return View(model);
            }

            if (model.Timestamp.HasValue && model.Timestamp > DateTime.Now)
            {
                ModelState.AddModelError("Timestamp", "Tarih gelecekte olamaz.");
                return View(model);
            }

            model.UserFullName = $"{user.FirstName} {user.LastName}";
            model.AuthorizedDoors = _context.UserDoors
                .Where(ud => ud.UserID == user.UserID)
                .Select(ud => ud.Door.DoorName)
                .ToList();

            var log = new Log
            {
                UserID = user.UserID,
                DoorID = door.DoorID,
                ErrorStatus = model.ErrorStatus,
                Operation = model.IsEntry, // true = giriş, false = çıkış
                CardID = user.CardID,
                Timestamp = model.Timestamp ?? DateTime.Now
            };

            _context.Logs.Add(log);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Kayıt başarıyla eklendi.";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public JsonResult GetUserInfo(string cardId)
        {
            var user = _context.Users.FirstOrDefault(u => u.CardID == cardId);
            if (user == null)
                return Json(null);

            var doorNames = _context.UserDoors
                .Where(ud => ud.UserID == user.UserID && ud.AccessGranted)
                .Select(ud => ud.Door.DoorName)
                .ToList();

            return Json(new
            {
                fullName = $"{user.FirstName} {user.LastName}",
                authorizedDoors = doorNames
            });
        }
    }
}
