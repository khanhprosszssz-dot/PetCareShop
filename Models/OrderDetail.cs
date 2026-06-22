namespace PetCareShop.Models
{
    public class OrderDetail
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public Order? Order { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; } = "";

        public string ImageUrl { get; set; } = "";

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public decimal Total
        {
            get
            {
                return Price * Quantity;
            }
        }
    }
}
