using Microsoft.EntityFrameworkCore;
using PetCareShop.Data;
using PetCareShop.Models.Interfaces;

namespace PetCareShop.Models.Services
{
    public class ShoppingCartRepository
        : IShoppingCartRepository
    {
        private const string CartIdSessionKey =
            "ShoppingCartId";

        private readonly ApplicationDbContext _context;

        public string ShoppingCartId { get; }

        private ShoppingCartRepository(
            ApplicationDbContext context,
            string shoppingCartId)
        {
            _context = context;
            ShoppingCartId = shoppingCartId;
        }

        public static ShoppingCartRepository GetCart(
            IServiceProvider services)
        {
            var httpContextAccessor =
                services.GetRequiredService<
                    IHttpContextAccessor>();

            var httpContext =
                httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException(
                    "Không tìm thấy HttpContext.");

            var context =
                services.GetRequiredService<
                    ApplicationDbContext>();

            string? cartId =
                httpContext.Session.GetString(
                    CartIdSessionKey);

            if (string.IsNullOrWhiteSpace(cartId))
            {
                cartId = Guid.NewGuid().ToString();

                httpContext.Session.SetString(
                    CartIdSessionKey,
                    cartId);
            }

            return new ShoppingCartRepository(
                context,
                cartId);
        }

        public async Task AddToCartAsync(
            Product product)
        {
            var cartItem =
                await _context.ShoppingCartItems
                    .FirstOrDefaultAsync(item =>
                        item.ShoppingCartId ==
                            ShoppingCartId &&
                        item.ProductId ==
                            product.Id);

            if (cartItem == null)
            {
                cartItem = new ShoppingCartItem
                {
                    ShoppingCartId =
                        ShoppingCartId,

                    ProductId =
                        product.Id,

                    Quantity = 1
                };

                await _context.ShoppingCartItems
                    .AddAsync(cartItem);
            }
            else
            {
                cartItem.Quantity++;
            }

            await _context.SaveChangesAsync();
        }

        public async Task IncreaseQuantityAsync(
            int productId)
        {
            var cartItem =
                await FindCartItemAsync(productId);

            if (cartItem == null)
            {
                return;
            }

            cartItem.Quantity++;

            await _context.SaveChangesAsync();
        }

        public async Task<int> DecreaseQuantityAsync(
            int productId)
        {
            var cartItem =
                await FindCartItemAsync(productId);

            if (cartItem == null)
            {
                return 0;
            }

            if (cartItem.Quantity > 1)
            {
                cartItem.Quantity--;

                await _context.SaveChangesAsync();

                return cartItem.Quantity;
            }

            _context.ShoppingCartItems.Remove(
                cartItem);

            await _context.SaveChangesAsync();

            return 0;
        }

        public async Task RemoveFromCartAsync(
            int productId)
        {
            var cartItem =
                await FindCartItemAsync(productId);

            if (cartItem == null)
            {
                return;
            }

            _context.ShoppingCartItems.Remove(
                cartItem);

            await _context.SaveChangesAsync();
        }

        public async Task<List<ShoppingCartItem>>
            GetAllShoppingCartItemsAsync()
        {
            return await _context.ShoppingCartItems
                .AsNoTracking()
                .Where(item =>
                    item.ShoppingCartId ==
                    ShoppingCartId)
                .Include(item => item.Product)
                .OrderByDescending(item => item.Id)
                .ToListAsync();
        }

        public async Task<decimal>
            GetShoppingCartTotalAsync()
        {
            return await _context.ShoppingCartItems
                .Where(item =>
                    item.ShoppingCartId ==
                    ShoppingCartId)
                .Select(item =>
                    (decimal?)(
                        item.Product.Price *
                        item.Quantity))
                .SumAsync() ?? 0;
        }

        public async Task<int>
            GetShoppingCartCountAsync()
        {
            return await _context.ShoppingCartItems
                .Where(item =>
                    item.ShoppingCartId ==
                    ShoppingCartId)
                .Select(item =>
                    (int?)item.Quantity)
                .SumAsync() ?? 0;
        }

        public async Task ClearCartAsync()
        {
            var cartItems =
                await _context.ShoppingCartItems
                    .Where(item =>
                        item.ShoppingCartId ==
                        ShoppingCartId)
                    .ToListAsync();

            if (cartItems.Count == 0)
            {
                return;
            }

            _context.ShoppingCartItems.RemoveRange(
                cartItems);

            await _context.SaveChangesAsync();
        }

        private async Task<ShoppingCartItem?>
            FindCartItemAsync(int productId)
        {
            return await _context.ShoppingCartItems
                .FirstOrDefaultAsync(item =>
                    item.ShoppingCartId ==
                        ShoppingCartId &&
                    item.ProductId ==
                        productId);
        }
    }
}