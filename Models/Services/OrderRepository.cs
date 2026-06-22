using Microsoft.EntityFrameworkCore;
using PetCareShop.Data;
using PetCareShop.Models.Interfaces;

namespace PetCareShop.Models.Services
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _context;

        private readonly IShoppingCartRepository
            _shoppingCartRepository;

        public OrderRepository(
            ApplicationDbContext context,
            IShoppingCartRepository shoppingCartRepository)
        {
            _context = context;
            _shoppingCartRepository =
                shoppingCartRepository;
        }

        public async Task<Order> PlaceOrderAsync(
            Order order)
        {
            var cartItems =
                await _shoppingCartRepository
                    .GetAllShoppingCartItemsAsync();

            if (cartItems.Count == 0)
            {
                throw new InvalidOperationException(
                    "Giỏ hàng đang trống.");
            }

            order.OrderDate = DateTime.Now;
            order.Status = "Chờ xác nhận";

            order.TotalAmount =
                cartItems.Sum(item =>
                    item.Product.Price *
                    item.Quantity);

            order.OrderDetails =
                cartItems.Select(item =>
                    new OrderDetail
                    {
                        ProductId =
                            item.ProductId,

                        ProductName =
                            item.Product.Name,

                        ImageUrl =
                            item.Product.ImageUrl,

                        Price =
                            item.Product.Price,

                        Quantity =
                            item.Quantity
                    })
                .ToList();

            await _context.Orders.AddAsync(order);

            await _context.SaveChangesAsync();

            await _shoppingCartRepository
                .ClearCartAsync();

            return order;
        }

        public async Task<Order?>
            GetOrderByIdAsync(int id)
        {
            return await _context.Orders
                .AsNoTracking()
                .Include(order =>
                    order.OrderDetails)
                .Include(order =>
                    order.Customer)
                .FirstOrDefaultAsync(order =>
                    order.Id == id);
        }

        public async Task<List<Order>>
            GetOrdersByCustomerIdAsync(
                int customerId)
        {
            return await _context.Orders
                .AsNoTracking()
                .Where(order =>
                    order.CustomerId ==
                    customerId)
                .Include(order =>
                    order.OrderDetails)
                .OrderByDescending(order =>
                    order.OrderDate)
                .ToListAsync();
        }
    }
}