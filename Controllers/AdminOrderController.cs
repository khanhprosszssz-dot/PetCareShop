using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCareShop.Data;

namespace PetCareShop.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminOrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminOrderController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            string? search,
            string? status,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var query =
                _context.Orders.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(order =>
                    order.FullName.Contains(search) ||
                    order.Phone.Contains(search) ||
                    order.Email.Contains(search) ||
                    order.Address.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(order =>
                    order.Status == status);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(order =>
                    order.OrderDate.Date >=
                    fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                query = query.Where(order =>
                    order.OrderDate.Date <=
                    toDate.Value.Date);
            }

            ViewBag.Search = search;
            ViewBag.Status = status;

            ViewBag.FromDate =
                fromDate?.ToString("yyyy-MM-dd");

            ViewBag.ToDate =
                toDate?.ToString("yyyy-MM-dd");

            var orders =
                await query
                    .OrderByDescending(order =>
                        order.OrderDate)
                    .ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var order =
                await _context.Orders
                    .Include(item => item.OrderDetails)
                    .Include(item => item.Customer)
                    .FirstOrDefaultAsync(item =>
                        item.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(
            int id,
            string status)
        {
            var allowedStatuses = new[]
            {
                "Chờ xác nhận",
                "Đang xử lý",
                "Đang giao",
                "Hoàn thành",
                "Đã hủy"
            };

            if (!allowedStatuses.Contains(status))
            {
                return BadRequest(
                    "Trạng thái đơn hàng không hợp lệ.");
            }

            var order =
                await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            order.Status = status;

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Detail),
                new { id });
        }
    }
}