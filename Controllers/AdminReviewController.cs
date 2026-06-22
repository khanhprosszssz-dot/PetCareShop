using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCareShop.Data;

namespace PetCareShop.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminReviewController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminReviewController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            int? productId,
            int? rating)
        {
            var query =
                _context.ProductReviews
                    .Include(review => review.Product)
                    .Include(review => review.Customer)
                    .AsQueryable();

            if (productId.HasValue)
            {
                query = query.Where(review =>
                    review.ProductId ==
                    productId.Value);
            }

            if (rating.HasValue)
            {
                query = query.Where(review =>
                    review.Rating == rating.Value);
            }

            ViewBag.Products =
                await _context.Products
                    .OrderBy(product =>
                        product.Name)
                    .ToListAsync();

            ViewBag.ProductId = productId;
            ViewBag.Rating = rating;

            var reviews =
                await query
                    .OrderByDescending(review =>
                        review.CreatedAt)
                    .ToListAsync();

            return View(reviews);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var review =
                await _context.ProductReviews
                    .Include(item => item.Product)
                    .Include(item => item.Customer)
                    .FirstOrDefaultAsync(item =>
                        item.Id == id);

            if (review == null)
            {
                return NotFound();
            }

            return View(review);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var review =
                await _context.ProductReviews
                    .Include(item => item.Product)
                    .Include(item => item.Customer)
                    .FirstOrDefaultAsync(item =>
                        item.Id == id);

            if (review == null)
            {
                return NotFound();
            }

            return View(review);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id)
        {
            var review =
                await _context.ProductReviews
                    .FindAsync(id);

            if (review != null)
            {
                _context.ProductReviews.Remove(review);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}