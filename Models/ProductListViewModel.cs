namespace PetCareShop.Models
{
    public class ProductListViewModel
    {
        public List<Product> Products { get; set; } = new List<Product>();

        public int CurrentPage { get; set; }

        public int TotalPages { get; set; }

        public string? Category { get; set; }

        public string? Search { get; set; }

        public decimal? MinPrice { get; set; }

        public decimal? MaxPrice { get; set; }

        public string? Sort { get; set; }
    }
}
