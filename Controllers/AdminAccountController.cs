using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PetCareShop.Controllers
{
    public class AdminAccountController : Controller
    {
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(
            string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true &&
                User.IsInRole("Admin"))
            {
                return RedirectToAction(
                    "Index",
                    "AdminDashboard");
            }

            string destination =
                "/Identity/Account/Login?returnUrl=" +
                Uri.EscapeDataString(
                    returnUrl ?? "/AdminDashboard");

            return Redirect(destination);
        }

        [HttpGet]
        public IActionResult Logout()
        {
            return Redirect(
                "/Identity/Account/Logout");
        }
    }
}