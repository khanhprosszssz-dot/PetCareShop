using System.ComponentModel.DataAnnotations;

namespace PetCareShop.Models
{
    public class ProductReview
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public Product? Product { get; set; }

        public int? CustomerId { get; set; }

        public Customer? Customer { get; set; }

        [Required]
        public string CustomerName { get; set; } = "";

        [Required]
        public string Comment { get; set; } = "";

        public int Rating { get; set; } = 5;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
