using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCareShop.Data;

namespace PetCareShop.Controllers
{
    public class AdminDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsAdminLoggedIn()
        {
            return HttpContext.Session.GetString("AdminLogin") == "true";
        }

        public async Task<IActionResult> Index()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "AdminAccount");
            }

            ViewBag.TotalProducts = await _context.Products.CountAsync();

            ViewBag.TotalOrders = await _context.Orders.CountAsync();

            ViewBag.PendingOrders = await _context.Orders
                .CountAsync(x => x.Status == "Chờ xác nhận");

            ViewBag.TotalReviews = await _context.ProductReviews.CountAsync();

            ViewBag.TotalRevenue = await _context.Orders
                .Where(x => x.Status != "Đã hủy")
                .Select(x => (decimal?)x.TotalAmount)
                .SumAsync() ?? 0;

            var latestOrders = await _context.Orders
                .OrderByDescending(x => x.OrderDate)
                .Take(5)
                .ToListAsync();

            return View(latestOrders);
        }
    }
}