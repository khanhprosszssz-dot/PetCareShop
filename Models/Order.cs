using System.ComponentModel.DataAnnotations;

namespace PetCareShop.Models
{
    public class Order
    {
        public int Id { get; set; }

        public int? CustomerId { get; set; }

        public Customer? Customer { get; set; }

        [Required]
        public string FullName { get; set; } = "";

        [Required]
        public string Phone { get; set; } = "";

        public string Email { get; set; } = "";

        [Required]
        public string Address { get; set; } = "";

        public string Note { get; set; } = "";

        public decimal TotalAmount { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        public string Status { get; set; } = "Chờ xác nhận";

        public List<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
