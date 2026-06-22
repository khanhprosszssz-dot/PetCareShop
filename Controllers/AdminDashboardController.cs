using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCareShop.Data;

namespace PetCareShop.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminDashboardController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalProducts =
                await _context.Products.CountAsync();

            ViewBag.TotalOrders =
                await _context.Orders.CountAsync();

            ViewBag.PendingOrders =
                await _context.Orders.CountAsync(
                    order =>
                        order.Status == "Chờ xác nhận");

            ViewBag.TotalReviews =
                await _context.ProductReviews.CountAsync();

            ViewBag.TotalRevenue =
                await _context.Orders
                    .Where(order =>
                        order.Status != "Đã hủy")
                    .Select(order =>
                        (decimal?)order.TotalAmount)
                    .SumAsync() ?? 0;

            var latestOrders =
                await _context.Orders
                    .OrderByDescending(order =>
                        order.OrderDate)
                    .Take(5)
                    .ToListAsync();

            return View(latestOrders);
        }
    }
}