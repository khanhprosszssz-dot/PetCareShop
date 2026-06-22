using PetCareShop.Models;

namespace PetCareShop.Models.Interfaces
{
    public interface IShoppingCartRepository
    {
        string ShoppingCartId { get; }

        Task AddToCartAsync(Product product);

        Task IncreaseQuantityAsync(int productId);

        Task<int> DecreaseQuantityAsync(int productId);

        Task RemoveFromCartAsync(int productId);

        Task<List<ShoppingCartItem>>
            GetAllShoppingCartItemsAsync();

        Task<decimal> GetShoppingCartTotalAsync();

        Task<int> GetShoppingCartCountAsync();

        Task ClearCartAsync();
    }
}