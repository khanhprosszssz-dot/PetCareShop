using PetCareShop.Models;

namespace PetCareShop.Models.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order> PlaceOrderAsync(Order order);

        Task<Order?> GetOrderByIdAsync(int id);

        Task<List<Order>> GetOrdersByCustomerIdAsync(
            int customerId);
    }
}