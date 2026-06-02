using Microsoft.AspNetCore.Mvc;
using PetCareShop.Models;

namespace PetCareShop.Controllers
{
    public class AdminAccountController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(AdminLogin model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Tài khoản admin tạm thời
            string adminUsername = "admin";
            string adminPassword = "123456";

            if (model.Username == adminUsername && model.Password == adminPassword)
            {
                HttpContext.Session.SetString("AdminLogin", "true");
                HttpContext.Session.SetString("AdminName", model.Username);

                return RedirectToAction("Index", "AdminDashboard");
            }

            ViewBag.Error = "Tài khoản hoặc mật khẩu không đúng.";
            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("AdminLogin");
            HttpContext.Session.Remove("AdminName");

            return RedirectToAction("Login");
        }
    }
}