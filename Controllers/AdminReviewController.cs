using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCareShop.Data;

namespace PetCareShop.Controllers
{
    public class AdminReviewController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminReviewController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsAdminLoggedIn()
        {
            return HttpContext.Session.GetString("AdminLogin") == "true";
        }

        public async Task<IActionResult> Index(int? productId, int? rating)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "AdminAccount");
            }

            var query = _context.ProductReviews
                .Include(x => x.Product)
                .Include(x => x.Customer)
                .AsQueryable();

            if (productId.HasValue)
            {
                query = query.Where(x => x.ProductId == productId.Value);
            }

            if (rating.HasValue)
            {
                query = query.Where(x => x.Rating == rating.Value);
            }

            ViewBag.Products = await _context.Products
                .OrderBy(x => x.Name)
                .ToListAsync();

            ViewBag.ProductId = productId;
            ViewBag.Rating = rating;

            var reviews = await query
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return View(reviews);
        }

        public async Task<IActionResult> Detail(int id)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "AdminAccount");
            }

            var review = await _context.ProductReviews
                .Include(x => x.Product)
                .Include(x => x.Customer)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (review == null)
            {
                return NotFound();
            }

            return View(review);
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "AdminAccount");
            }

            var review = await _context.ProductReviews
                .Include(x => x.Product)
                .Include(x => x.Customer)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (review == null)
            {
                return NotFound();
            }

            return View(review);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "AdminAccount");
            }

            var review = await _context.ProductReviews.FindAsync(id);

            if (review != null)
            {
                _context.ProductReviews.Remove(review);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }
    }
}