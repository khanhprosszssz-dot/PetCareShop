using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCareShop.Data;
using PetCareShop.Models;
using PetCareShop.Models.Interfaces;

namespace PetCareShop.Controllers
{
    public class CartController : Controller
    {
        private readonly IShoppingCartRepository
            _shoppingCartRepository;

        private readonly IProductRepository
            _productRepository;

        private readonly IOrderRepository
            _orderRepository;

        private readonly ApplicationDbContext
            _context;

        private readonly UserManager<ApplicationUser>
            _userManager;

        public CartController(
            IShoppingCartRepository shoppingCartRepository,
            IProductRepository productRepository,
            IOrderRepository orderRepository,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _shoppingCartRepository =
                shoppingCartRepository;

            _productRepository =
                productRepository;

            _orderRepository =
                orderRepository;

            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var cartItems =
                await _shoppingCartRepository
                    .GetAllShoppingCartItemsAsync();

            ViewBag.Total =
                await _shoppingCartRepository
                    .GetShoppingCartTotalAsync();

            return View(cartItems);
        }

        [HttpGet]
        public async Task<IActionResult> Add(int id)
        {
            var product =
                await _productRepository
                    .GetProductByIdAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            await _shoppingCartRepository
                .AddToCartAsync(product);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Increase(int id)
        {
            await _shoppingCartRepository
                .IncreaseQuantityAsync(id);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Decrease(int id)
        {
            await _shoppingCartRepository
                .DecreaseQuantityAsync(id);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Remove(int id)
        {
            await _shoppingCartRepository
                .RemoveFromCartAsync(id);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Clear()
        {
            await _shoppingCartRepository
                .ClearCartAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var cartItems =
                await _shoppingCartRepository
                    .GetAllShoppingCartItemsAsync();

            if (cartItems.Count == 0)
            {
                return RedirectToAction(nameof(Index));
            }

            var user =
                await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var customer =
                await GetOrCreateCustomerAsync(user);

            ViewBag.FullName = customer.FullName;
            ViewBag.Phone = customer.Phone;
            ViewBag.Email = customer.Email;
            ViewBag.Address = customer.Address;

            ViewBag.Total =
                await _shoppingCartRepository
                    .GetShoppingCartTotalAsync();

            return View(cartItems);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmOrder(
            string FullName,
            string Phone,
            string? Email,
            string Address,
            string? Note)
        {
            var cartItems =
                await _shoppingCartRepository
                    .GetAllShoppingCartItemsAsync();

            if (cartItems.Count == 0)
            {
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(FullName) ||
                string.IsNullOrWhiteSpace(Phone) ||
                string.IsNullOrWhiteSpace(Address))
            {
                TempData["Error"] =
                    "Vui lòng nhập đầy đủ họ tên, số điện thoại và địa chỉ.";

                return RedirectToAction(nameof(Checkout));
            }

            var user =
                await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var customer =
                await GetOrCreateCustomerAsync(user);

            var order = new Order
            {
                CustomerId = customer.Id,

                FullName = FullName.Trim(),

                Phone = Phone.Trim(),

                Email = Email?.Trim()
                    ?? user.Email
                    ?? string.Empty,

                Address = Address.Trim(),

                Note = Note?.Trim()
                    ?? string.Empty
            };

            try
            {
                order =
                    await _orderRepository
                        .PlaceOrderAsync(order);
            }
            catch (InvalidOperationException exception)
            {
                TempData["Error"] =
                    exception.Message;

                return RedirectToAction(nameof(Index));
            }

            TempData["OrderId"] = order.Id;

            return RedirectToAction(nameof(Success));
        }

        [HttpGet]
        public IActionResult Success()
        {
            return View();
        }

        private async Task<Customer>
            GetOrCreateCustomerAsync(
                ApplicationUser user)
        {
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                throw new InvalidOperationException(
                    "Tài khoản chưa có email.");
            }

            string email =
                user.Email.Trim().ToLower();

            var customer =
                await _context.Customers
                    .FirstOrDefaultAsync(item =>
                        item.Email == email);

            if (customer == null)
            {
                customer = new Customer
                {
                    FullName = user.FullName,

                    Phone =
                        user.PhoneNumber
                        ?? string.Empty,

                    Email = email,

                    Address = user.Address,

                    CreatedAt = user.CreatedAt
                };

                _context.Customers.Add(customer);
            }
            else
            {
                customer.FullName =
                    user.FullName;

                customer.Phone =
                    user.PhoneNumber
                    ?? customer.Phone;

                customer.Address =
                    user.Address;
            }

            await _context.SaveChangesAsync();

            return customer;
        }
    }
}