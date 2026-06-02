using System.ComponentModel.DataAnnotations;

namespace PetCareShop.Models
{
    public class CustomerLogin
    {
        [Required(ErrorMessage = "Vui lòng nhập email")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        public string Password { get; set; } = "";
    }
}