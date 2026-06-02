using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCareShop.Data;

namespace PetCareShop.Controllers
{
    public class CustomerOrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerOrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsCustomerLoggedIn()
        {
            return HttpContext.Session.GetString("CustomerLogin") == "true";
        }

        public async Task<IActionResult> Index()
        {
            if (!IsCustomerLoggedIn())
            {
                return RedirectToAction("Login", "CustomerAccount");
            }

            var customerId = HttpContext.Session.GetInt32("CustomerId");

            if (customerId == null)
            {
                return RedirectToAction("Login", "CustomerAccount");
            }

            var orders = await _context.Orders
                .Where(x => x.CustomerId == customerId.Value)
                .OrderByDescending(x => x.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> Detail(int id)
        {
            if (!IsCustomerLoggedIn())
            {
                return RedirectToAction("Login", "CustomerAccount");
            }

            var customerId = HttpContext.Session.GetInt32("CustomerId");

            if (customerId == null)
            {
                return RedirectToAction("Login", "CustomerAccount");
            }

            var order = await _context.Orders
                .Include(x => x.OrderDetails)
                .FirstOrDefaultAsync(x => x.Id == id && x.CustomerId == customerId.Value);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            if (!IsCustomerLoggedIn())
            {
                return RedirectToAction("Login", "CustomerAccount");
            }

            var customerId = HttpContext.Session.GetInt32("CustomerId");

            if (customerId == null)
            {
                return RedirectToAction("Login", "CustomerAccount");
            }

            var order = await _context.Orders
                .FirstOrDefaultAsync(x => x.Id == id && x.CustomerId == customerId.Value);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status == "Chờ xác nhận")
            {
                order.Status = "Đã hủy";
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Detail", new { id = id });
        }
    }
}