using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCareShop.Data;
using PetCareShop.Extensions;
using PetCareShop.Models;

namespace PetCareShop.Controllers
{
    public class CartController : Controller
    {
        private const string CartSessionKey = "CART";
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var cart = GetCart();

            ViewBag.Total = cart.Sum(x => x.Total);

            return View(cart);
        }

        public async Task<IActionResult> Add(int id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            var cart = GetCart();

            var item = cart.FirstOrDefault(x => x.ProductId == id);

            if (item == null)
            {
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    ImageUrl = product.ImageUrl,
                    Price = product.Price,
                    Quantity = 1
                });
            }
            else
            {
                item.Quantity++;
            }

            SaveCart(cart);

            return RedirectToAction("Index");
        }

        public IActionResult Increase(int id)
        {
            var cart = GetCart();

            var item = cart.FirstOrDefault(x => x.ProductId == id);

            if (item != null)
            {
                item.Quantity++;
            }

            SaveCart(cart);

            return RedirectToAction("Index");
        }

        public IActionResult Decrease(int id)
        {
            var cart = GetCart();

            var item = cart.FirstOrDefault(x => x.ProductId == id);

            if (item != null)
            {
                item.Quantity--;

                if (item.Quantity <= 0)
                {
                    cart.Remove(item);
                }
            }

            SaveCart(cart);

            return RedirectToAction("Index");
        }

        public IActionResult Remove(int id)
        {
            var cart = GetCart();

            var item = cart.FirstOrDefault(x => x.ProductId == id);

            if (item != null)
            {
                cart.Remove(item);
            }

            SaveCart(cart);

            return RedirectToAction("Index");
        }

        public IActionResult Clear()
        {
            SaveCart(new List<CartItem>());

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Checkout()
        {
            var cart = GetCart();

            if (cart.Count == 0)
            {
                return RedirectToAction("Index");
            }

            var customerId = HttpContext.Session.GetInt32("CustomerId");

            if (customerId != null)
            {
                var customer = await _context.Customers.FindAsync(customerId.Value);

                if (customer != null)
                {
                    ViewBag.FullName = customer.FullName;
                    ViewBag.Phone = customer.Phone;
                    ViewBag.Email = customer.Email;
                    ViewBag.Address = customer.Address;
                }
            }

            ViewBag.Total = cart.Sum(x => x.Total);

            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmOrder(
            string FullName,
            string Phone,
            string Email,
            string Address,
            string Note)
        {
            var cart = GetCart();

            if (cart.Count == 0)
            {
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(FullName) ||
                string.IsNullOrWhiteSpace(Phone) ||
                string.IsNullOrWhiteSpace(Address))
            {
                TempData["Error"] = "Vui lòng nhập đầy đủ họ tên, số điện thoại và địa chỉ nhận hàng.";
                return RedirectToAction("Checkout");
            }

            var order = new Order
            {
                CustomerId = HttpContext.Session.GetInt32("CustomerId"),
                FullName = FullName,
                Phone = Phone,
                Email = Email ?? "",
                Address = Address,
                Note = Note ?? "",
                OrderDate = DateTime.Now,
                Status = "Chờ xác nhận",
                TotalAmount = cart.Sum(x => x.Total),
                OrderDetails = cart.Select(x => new OrderDetail
                {
                    ProductId = x.ProductId,
                    ProductName = x.ProductName,
                    ImageUrl = x.ImageUrl,
                    Price = x.Price,
                    Quantity = x.Quantity
                }).ToList()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            SaveCart(new List<CartItem>());

            TempData["OrderId"] = order.Id;

            return RedirectToAction("Success");
        }

        public IActionResult Success()
        {
            return View();
        }

        private List<CartItem> GetCart()
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>(CartSessionKey);

            if (cart == null)
            {
                cart = new List<CartItem>();
            }

            return cart;
        }

        private void SaveCart(List<CartItem> cart)
        {
            HttpContext.Session.SetObject(CartSessionKey, cart);
        }
    }
}