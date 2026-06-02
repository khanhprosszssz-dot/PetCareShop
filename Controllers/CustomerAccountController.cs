using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCareShop.Data;
using PetCareShop.Models;

namespace PetCareShop.Controllers
{
    public class CustomerAccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerAccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(Customer customer)
        {
            if (await _context.Customers.AnyAsync(x => x.Email == customer.Email))
            {
                ModelState.AddModelError("Email", "Email này đã được đăng ký");
            }

            if (ModelState.IsValid)
            {
                customer.CreatedAt = DateTime.Now;

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                HttpContext.Session.SetString("CustomerLogin", "true");
                HttpContext.Session.SetInt32("CustomerId", customer.Id);
                HttpContext.Session.SetString("CustomerName", customer.FullName);

                return RedirectToAction("Index", "Home");
            }

            return View(customer);
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(CustomerLogin model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(x => x.Email == model.Email && x.Password == model.Password);

            if (customer == null)
            {
                ViewBag.Error = "Email hoặc mật khẩu không đúng.";
                return View(model);
            }

            HttpContext.Session.SetString("CustomerLogin", "true");
            HttpContext.Session.SetInt32("CustomerId", customer.Id);
            HttpContext.Session.SetString("CustomerName", customer.FullName);

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("CustomerLogin");
            HttpContext.Session.Remove("CustomerId");
            HttpContext.Session.Remove("CustomerName");

            return RedirectToAction("Index", "Home");
        }
    }
}