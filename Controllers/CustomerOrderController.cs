using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCareShop.Data;
using PetCareShop.Models;
using PetCareShop.Models.Interfaces;

namespace PetCareShop.Controllers
{
    [Authorize]
    public class CustomerOrderController : Controller
    {
        private readonly ApplicationDbContext
            _context;

        private readonly UserManager<ApplicationUser>
            _userManager;

        private readonly IOrderRepository
            _orderRepository;

        public CustomerOrderController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IOrderRepository orderRepository)
        {
            _context = context;
            _userManager = userManager;
            _orderRepository = orderRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var customer =
                await GetCurrentCustomerAsync();

            if (customer == null)
            {
                return View(new List<Order>());
            }

            var orders =
                await _orderRepository
                    .GetOrdersByCustomerIdAsync(
                        customer.Id);

            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var customer =
                await GetCurrentCustomerAsync();

            if (customer == null)
            {
                return NotFound();
            }

            var order =
                await _orderRepository
                    .GetOrderByIdAsync(id);

            if (order == null ||
                order.CustomerId != customer.Id)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var customer =
                await GetCurrentCustomerAsync();

            if (customer == null)
            {
                return NotFound();
            }

            var order =
                await _context.Orders
                    .FirstOrDefaultAsync(item =>
                        item.Id == id &&
                        item.CustomerId ==
                        customer.Id);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status == "Chờ xác nhận")
            {
                order.Status = "Đã hủy";

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Hủy đơn hàng thành công.";
            }
            else
            {
                TempData["Error"] =
                    "Chỉ có thể hủy đơn đang chờ xác nhận.";
            }

            return RedirectToAction(
                nameof(Detail),
                new { id });
        }

        private async Task<Customer?>
            GetCurrentCustomerAsync()
        {
            var user =
                await _userManager.GetUserAsync(User);

            if (user == null ||
                string.IsNullOrWhiteSpace(user.Email))
            {
                return null;
            }

            string email =
                user.Email.Trim().ToLower();

            return await _context.Customers
                .FirstOrDefaultAsync(customer =>
                    customer.Email == email);
        }
    }
}