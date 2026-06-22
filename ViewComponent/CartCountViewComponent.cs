using Microsoft.AspNetCore.Mvc;
using PetCareShop.Models.Interfaces;

namespace PetCareShop.ViewComponents
{
    public class CartCountViewComponent
        : ViewComponent
    {
        private readonly IShoppingCartRepository
            _shoppingCartRepository;

        public CartCountViewComponent(
            IShoppingCartRepository
                shoppingCartRepository)
        {
            _shoppingCartRepository =
                shoppingCartRepository;
        }

        public async Task<IViewComponentResult>
            InvokeAsync()
        {
            int count =
                await _shoppingCartRepository
                    .GetShoppingCartCountAsync();

            return View(count);
        }
    }
}