using Microsoft.EntityFrameworkCore;
using PetCareShop.Data;
using PetCareShop.Models.Interfaces;

namespace PetCareShop.Models.Services
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(List<Product> Products, int TotalItems)>
            GetPagedProductsAsync(
                string? category,
                string? search,
                decimal? minPrice,
                decimal? maxPrice,
                string? sort,
                int page,
                int pageSize)
        {
            IQueryable<Product> query =
                _context.Products.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(product =>
                    product.Category == category);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                string keyword = search.Trim();

                query = query.Where(product =>
                    product.Name.Contains(keyword) ||
                    product.Description.Contains(keyword) ||
                    product.Category.Contains(keyword));
            }

            if (minPrice.HasValue)
            {
                query = query.Where(product =>
                    product.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(product =>
                    product.Price <= maxPrice.Value);
            }

            query = sort switch
            {
                "price_asc" =>
                    query.OrderBy(product => product.Price),

                "price_desc" =>
                    query.OrderByDescending(product => product.Price),

                "name_asc" =>
                    query.OrderBy(product => product.Name),

                "name_desc" =>
                    query.OrderByDescending(product => product.Name),

                _ =>
                    query.OrderByDescending(product => product.Id)
            };

            int totalItems = await query.CountAsync();

            List<Product> products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (products, totalItems);
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            return await _context.Products
                .AsNoTracking()
                .OrderByDescending(product => product.Id)
                .ToListAsync();
        }

        public async Task<List<string>> GetCategoriesAsync()
        {
            return await _context.Products
                .AsNoTracking()
                .Select(product => product.Category)
                .Where(category => category != "")
                .Distinct()
                .OrderBy(category => category)
                .ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _context.Products
                .FirstOrDefaultAsync(product =>
                    product.Id == id);
        }

        public async Task<Product?> GetProductByIdAsNoTrackingAsync(
            int id)
        {
            return await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(product =>
                    product.Id == id);
        }

        public async Task AddProductAsync(Product product)
        {
            await _context.Products.AddAsync(product);
        }

        public void UpdateProduct(Product product)
        {
            _context.Products.Update(product);
        }

        public void DeleteProduct(Product product)
        {
            _context.Products.Remove(product);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}