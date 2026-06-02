using System.ComponentModel.DataAnnotations;

namespace PetCareShop.Models
{
    public class AdminLogin
    {
        [Required(ErrorMessage = "Vui lòng nhập tài khoản")]
        public string Username { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        public string Password { get; set; } = "";
    }
}