using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace PetCareShop.Data
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(150)]
        public string FullName { get; set; } = string.Empty;

        [StringLength(300)]
        public string Address { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
