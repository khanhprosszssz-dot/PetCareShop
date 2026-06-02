using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCareShop.Data;
using PetCareShop.Models;

namespace PetCareShop.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            string? category,
            string? search,
            decimal? minPrice,
            decimal? maxPrice,
            string? sort,
            int page = 1)
        {
            int pageSize = 6;

            if (page < 1)
            {
                page = 1;
            }

            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(p => p.Category == category);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    p.Name.Contains(search) ||
                    p.Description.Contains(search) ||
                    p.Category.Contains(search));
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            switch (sort)
            {
                case "price_asc":
                    query = query.OrderBy(p => p.Price);
                    break;

                case "price_desc":
                    query = query.OrderByDescending(p => p.Price);
                    break;

                case "name_asc":
                    query = query.OrderBy(p => p.Name);
                    break;

                case "name_desc":
                    query = query.OrderByDescending(p => p.Name);
                    break;

                default:
                    query = query.OrderByDescending(p => p.Id);
                    break;
            }

            var totalItems = await query.CountAsync();

            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Categories = await _context.Products
                .Select(p => p.Category)
                .Distinct()
                .ToListAsync();

            var model = new ProductListViewModel
            {
                Products = products,
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
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            var reviews = await _context.ProductReviews
                .Where(x => x.ProductId == id)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            double averageRating = 0;

            if (reviews.Count > 0)
            {
                averageRating = reviews.Average(x => x.Rating);
            }

            var model = new ProductDetailViewModel
            {
                Product = product,
                Reviews = reviews,
                AverageRating = averageRating,
                ReviewCount = reviews.Count,
                IsCustomerLoggedIn = HttpContext.Session.GetString("CustomerLogin") == "true",
                CustomerName = HttpContext.Session.GetString("CustomerName")
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateReview(
            int productId,
            int rating,
            string comment)
        {
            if (HttpContext.Session.GetString("CustomerLogin") != "true")
            {
                TempData["ReviewError"] = "Bạn cần đăng nhập khách hàng để đánh giá sản phẩm.";
                return RedirectToAction("Detail", new { id = productId });
            }

            var product = await _context.Products.FindAsync(productId);

            if (product == null)
            {
                return NotFound();
            }

            if (rating < 1 || rating > 5)
            {
                TempData["ReviewError"] = "Số sao đánh giá phải từ 1 đến 5.";
                return RedirectToAction("Detail", new { id = productId });
            }

            if (string.IsNullOrWhiteSpace(comment))
            {
                TempData["ReviewError"] = "Vui lòng nhập nội dung đánh giá.";
                return RedirectToAction("Detail", new { id = productId });
            }

            var customerId = HttpContext.Session.GetInt32("CustomerId");
            var customerName = HttpContext.Session.GetString("CustomerName") ?? "Khách hàng";

            var review = new ProductReview
            {
                ProductId = productId,
                CustomerId = customerId,
                CustomerName = customerName,
                Rating = rating,
                Comment = comment,
                CreatedAt = DateTime.Now
            };

            _context.ProductReviews.Add(review);
            await _context.SaveChangesAsync();

            TempData["ReviewSuccess"] = "Cảm ơn bạn đã đánh giá sản phẩm.";

            return RedirectToAction("Detail", new { id = productId });
        }
    }
}