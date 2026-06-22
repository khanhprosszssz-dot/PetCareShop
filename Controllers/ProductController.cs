using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCareShop.Data;
using PetCareShop.Models;
using PetCareShop.Models.Interfaces;

namespace PetCareShop.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductController(
            IProductRepository productRepository,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _productRepository = productRepository;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(
            string? category,
            string? search,
            decimal? minPrice,
            decimal? maxPrice,
            string? sort,
            int page = 1)
        {
            const int pageSize = 6;

            if (page < 1)
            {
                page = 1;
            }

            var result =
                await _productRepository.GetPagedProductsAsync(
                    category,
                    search,
                    minPrice,
                    maxPrice,
                    sort,
                    page,
                    pageSize);

            int totalPages = (int)Math.Ceiling(
                result.TotalItems / (double)pageSize);

            ViewBag.Categories =
                await _productRepository.GetCategoriesAsync();

            var model = new ProductListViewModel
            {
                Products = result.Products,
                CurrentPage = page,
                TotalPages = totalPages,
                Category = category,
                Search = search,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                Sort = sort
            };

            return View(model);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var product =
                await _productRepository.GetProductByIdAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            var reviews = await _context.ProductReviews
                .Where(review => review.ProductId == id)
                .OrderByDescending(review => review.CreatedAt)
                .ToListAsync();

            double averageRating = reviews.Count > 0
                ? reviews.Average(review => review.Rating)
                : 0;

            var currentUser =
                await _userManager.GetUserAsync(User);

            var model = new ProductDetailViewModel
            {
                Product = product,
                Reviews = reviews,
                AverageRating = averageRating,
                ReviewCount = reviews.Count,
                IsCustomerLoggedIn = currentUser != null,
                CustomerName = currentUser?.FullName
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateReview(
            int productId,
            int rating,
            string comment)
        {
            var product =
                await _productRepository.GetProductByIdAsync(
                    productId);

            if (product == null)
            {
                return NotFound();
            }

            if (rating < 1 || rating > 5)
            {
                TempData["ReviewError"] =
                    "Số sao đánh giá phải từ 1 đến 5.";

                return RedirectToAction(
                    nameof(Detail),
                    new { id = productId });
            }

            if (string.IsNullOrWhiteSpace(comment))
            {
                TempData["ReviewError"] =
                    "Vui lòng nhập nội dung đánh giá.";

                return RedirectToAction(
                    nameof(Detail),
                    new { id = productId });
            }

            var user =
                await _userManager.GetUserAsync(User);

            if (user == null ||
                string.IsNullOrWhiteSpace(user.Email))
            {
                return Challenge();
            }

            string email = user.Email.Trim();

            var customer = await _context.Customers
                .FirstOrDefaultAsync(item =>
                    item.Email == email);

            if (customer == null)
            {
                customer = new Customer
                {
                    FullName = user.FullName,
                    Phone = user.PhoneNumber ?? string.Empty,
                    Email = email,
                    Address = user.Address,
                    CreatedAt = user.CreatedAt
                };

                _context.Customers.Add(customer);

                await _context.SaveChangesAsync();
            }

            var review = new ProductReview
            {
                ProductId = productId,
                CustomerId = customer.Id,
                CustomerName = customer.FullName,
                Rating = rating,
                Comment = comment.Trim(),
                CreatedAt = DateTime.Now
            };

            _context.ProductReviews.Add(review);

            await _context.SaveChangesAsync();

            TempData["ReviewSuccess"] =
                "Cảm ơn bạn đã đánh giá sản phẩm.";

            return RedirectToAction(
                nameof(Detail),
                new { id = productId });
        }
    }
}