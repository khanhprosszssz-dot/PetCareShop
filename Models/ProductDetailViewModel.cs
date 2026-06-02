namespace PetCareShop.Models
{
    public class ProductDetailViewModel
    {
        public Product Product { get; set; } = new Product();

        public List<ProductReview> Reviews { get; set; } = new List<ProductReview>();

        public double AverageRating { get; set; }

        public int ReviewCount { get; set; }

        public bool IsCustomerLoggedIn { get; set; }

        public string? CustomerName { get; set; }
    }
}