using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCareShop.Models;
using PetCareShop.Models.Interfaces;

namespace PetCareShop.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly IWebHostEnvironment _environment;

        public AdminProductController(
            IProductRepository productRepository,
            IWebHostEnvironment environment)
        {
            _productRepository = productRepository;
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            var products =
                await _productRepository.GetAllProductsAsync();

            return View(products);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Product());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (product.ImageFile != null)
            {
                try
                {
                    product.ImageUrl =
                        await SaveImageAsync(product.ImageFile);
                }
                catch (InvalidOperationException exception)
                {
                    ModelState.AddModelError(
                        nameof(product.ImageFile),
                        exception.Message);
                }
            }

            if (string.IsNullOrWhiteSpace(product.ImageUrl))
            {
                ModelState.AddModelError(
                    nameof(product.ImageFile),
                    "Vui lòng chọn ảnh sản phẩm.");
            }

            if (!ModelState.IsValid)
            {
                return View(product);
            }

            await _productRepository.AddProductAsync(product);
            await _productRepository.SaveChangesAsync();

            TempData["Success"] =
                "Thêm sản phẩm thành công.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product =
                await _productRepository.GetProductByIdAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product)
        {
            var oldProduct =
                await _productRepository
                    .GetProductByIdAsNoTrackingAsync(product.Id);

            if (oldProduct == null)
            {
                return NotFound();
            }

            string oldImageUrl = oldProduct.ImageUrl;

            if (product.ImageFile != null)
            {
                try
                {
                    product.ImageUrl =
                        await SaveImageAsync(product.ImageFile);
                }
                catch (InvalidOperationException exception)
                {
                    ModelState.AddModelError(
                        nameof(product.ImageFile),
                        exception.Message);
                }
            }
            else
            {
                product.ImageUrl = oldImageUrl;
            }

            if (!ModelState.IsValid)
            {
                return View(product);
            }

            _productRepository.UpdateProduct(product);
            await _productRepository.SaveChangesAsync();

            if (product.ImageFile != null &&
                product.ImageUrl != oldImageUrl)
            {
                DeleteImage(oldImageUrl);
            }

            TempData["Success"] =
                "Cập nhật sản phẩm thành công.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var product =
                await _productRepository.GetProductByIdAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product =
                await _productRepository.GetProductByIdAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            string imageUrl = product.ImageUrl;

            _productRepository.DeleteProduct(product);
            await _productRepository.SaveChangesAsync();

            DeleteImage(imageUrl);

            TempData["Success"] =
                "Xóa sản phẩm thành công.";

            return RedirectToAction(nameof(Index));
        }

        private async Task<string> SaveImageAsync(
            IFormFile imageFile)
        {
            string[] allowedExtensions =
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };

            string extension = Path
                .GetExtension(imageFile.FileName)
                .ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException(
                    "Chỉ chấp nhận ảnh JPG, JPEG, PNG hoặc WEBP.");
            }

            const long maximumSize = 5 * 1024 * 1024;

            if (imageFile.Length <= 0)
            {
                throw new InvalidOperationException(
                    "File ảnh không hợp lệ.");
            }

            if (imageFile.Length > maximumSize)
            {
                throw new InvalidOperationException(
                    "Ảnh không được vượt quá 5 MB.");
            }

            string folderPath = Path.Combine(
                _environment.WebRootPath,
                "images",
                "products");

            Directory.CreateDirectory(folderPath);

            string fileName =
                $"{Guid.NewGuid():N}{extension}";

            string filePath = Path.Combine(
                folderPath,
                fileName);

            await using var stream = new FileStream(
                filePath,
                FileMode.Create);

            await imageFile.CopyToAsync(stream);

            return $"/images/products/{fileName}";
        }

        private void DeleteImage(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return;
            }

            string fileName = Path.GetFileName(imageUrl);

            if (string.IsNullOrWhiteSpace(fileName))
            {
                return;
            }

            string filePath = Path.Combine(
                _environment.WebRootPath,
                "images",
                "products",
                fileName);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
    }
}