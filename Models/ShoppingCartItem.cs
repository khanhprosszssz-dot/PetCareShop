using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetCareShop.Models
{
    public class ShoppingCartItem
    {
        public int Id { get; set; }

        [Required]
        public string ShoppingCartId { get; set; } = string.Empty;

        public int ProductId { get; set; }

        public Product Product { get; set; } = null!;

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;

        [NotMapped]
        public decimal Total =>
            Product == null
                ? 0
                : Product.Price * Quantity;
    }
}