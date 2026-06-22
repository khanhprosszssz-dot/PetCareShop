using PetCareShop.Models;

namespace PetCareShop.Models.Interfaces
{
    public interface IProductRepository
    {
        Task<(List<Product> Products, int TotalItems)> GetPagedProductsAsync(
            string? category,
            string? search,
            decimal? minPrice,
            decimal? maxPrice,
            string? sort,
            int page,
            int pageSize);

        Task<List<Product>> GetAllProductsAsync();

        Task<List<string>> GetCategoriesAsync();

        Task<Product?> GetProductByIdAsync(int id);

        Task<Product?> GetProductByIdAsNoTrackingAsync(int id);

        Task AddProductAsync(Product product);

        void UpdateProduct(Product product);

        void DeleteProduct(Product product);

        Task SaveChangesAsync();
    }
}