using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetCareShop.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        [Display(Name = "Tên sản phẩm")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập danh mục")]
        [Display(Name = "Danh mục")]
        public string Category { get; set; } = "";

        [Display(Name = "Mô tả")]
        public string Description { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập giá")]
        [Display(Name = "Giá")]
        public decimal Price { get; set; }

        [Display(Name = "Hình ảnh")]
        public string ImageUrl { get; set; } = "";

        [NotMapped]
        public IFormFile? ImageFile { get; set; }
    }
}
