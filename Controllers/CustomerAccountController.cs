using Microsoft.AspNetCore.Mvc;

namespace PetCareShop.Controllers
{
    public class CustomerAccountController : Controller
    {
        [HttpGet]
        public IActionResult Login(
            string? returnUrl = null)
        {
            string destination =
                "/Identity/Account/Login?returnUrl=" +
                Uri.EscapeDataString(returnUrl ?? "/");

            return Redirect(destination);
        }

        [HttpGet]
        public IActionResult Register(
            string? returnUrl = null)
        {
            string destination =
                "/Identity/Account/Register?returnUrl=" +
                Uri.EscapeDataString(returnUrl ?? "/");

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