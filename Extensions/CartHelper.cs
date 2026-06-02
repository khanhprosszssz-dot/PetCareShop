using PetCareShop.Models;

namespace PetCareShop.Extensions
{
    public static class CartHelper
    {
        private const string CartSessionKey = "CART";

        public static int GetCartCount(ISession session)
        {
            var cart = session.GetObject<List<CartItem>>(CartSessionKey);

            if (cart == null)
            {
                return 0;
            }

            return cart.Sum(x => x.Quantity);
        }
    }
}