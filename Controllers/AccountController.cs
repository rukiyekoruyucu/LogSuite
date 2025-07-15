using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartDoors.Data;
using SmartDoors.Models;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace SmartDoors.Controllers
{
    public class AccountController : Controller
    {
        private readonly SmartDoorContext _context;

        public AccountController(SmartDoorContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.LoginError = "Kullanıcı adı ve şifre gerekli.";
                return View();
            }

            var admins = await _context.Admins.ToListAsync();

            var admin = admins.FirstOrDefault(a =>
                (a.FirstName.Trim() + " " + a.LastName.Trim()).ToLower() == username.Trim().ToLower());

            if (admin == null)
            {
                ViewBag.LoginError = "Kullanıcı bulunamadı.";
                return View();
            }

            if (string.IsNullOrEmpty(admin.PasswordHash) || admin.PasswordHash != password)
            {
                ViewBag.LoginError = "Şifre hatalı.";
                return View();
            }

            HttpContext.Session.SetInt32("AdminID", admin.AdminID);

            return RedirectToAction("Index", "Home");
        }


        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
