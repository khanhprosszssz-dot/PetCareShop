using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PetCareShop.Data;
using PetCareShop.Models;

namespace PetCareShop.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly UserManager<ApplicationUser>
            _userManager;

        private readonly SignInManager<ApplicationUser>
            _signInManager;

        private readonly ApplicationDbContext
            _context;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
            [StringLength(150)]
            [Display(Name = "Họ và tên")]
            public string FullName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
            [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
            [StringLength(20)]
            [Display(Name = "Số điện thoại")]
            public string PhoneNumber { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập email.")]
            [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập địa chỉ.")]
            [StringLength(300)]
            [Display(Name = "Địa chỉ")]
            public string Address { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
            [StringLength(
                100,
                MinimumLength = 6,
                ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
            [DataType(DataType.Password)]
            [Display(Name = "Mật khẩu")]
            public string Password { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu.")]
            [DataType(DataType.Password)]
            [Compare(
                nameof(Password),
                ErrorMessage = "Mật khẩu xác nhận không khớp.")]
            [Display(Name = "Xác nhận mật khẩu")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public IActionResult OnGet(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return LocalRedirect(
                    Url.Content("~/"));
            }

            ReturnUrl =
                returnUrl ?? Url.Content("~/");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ReturnUrl ??= Url.Content("~/");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            string email =
                Input.Email.Trim().ToLower();

            var existingUser =
                await _userManager.FindByEmailAsync(email);

            if (existingUser != null)
            {
                ModelState.AddModelError(
                    nameof(Input.Email),
                    "Email này đã được đăng ký.");

                return Page();
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,

                FullName = Input.FullName.Trim(),
                PhoneNumber = Input.PhoneNumber.Trim(),
                Address = Input.Address.Trim(),
                CreatedAt = DateTime.Now
            };

            var createResult =
                await _userManager.CreateAsync(
                    user,
                    Input.Password);

            if (!createResult.Succeeded)
            {
                foreach (var error in createResult.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        TranslateIdentityError(
                            error.Code,
                            error.Description));
                }

                return Page();
            }

            var addRoleResult =
                await _userManager.AddToRoleAsync(
                    user,
                    "Customer");

            if (!addRoleResult.Succeeded)
            {
                foreach (var error in addRoleResult.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return Page();
            }

            var customer =
                await _context.Customers
                    .FirstOrDefaultAsync(
                        item => item.Email == email);

            if (customer == null)
            {
                customer = new Customer
                {
                    FullName = user.FullName,
                    Phone = user.PhoneNumber ?? string.Empty,
                    Email = email,
                    Address = user.Address,
                    CreatedAt = user.CreatedAt
                };

                _context.Customers.Add(customer);

                await _context.SaveChangesAsync();
            }

            await _signInManager.SignInAsync(
                user,
                isPersistent: false);

            return LocalRedirect(ReturnUrl);
        }

        private static string TranslateIdentityError(
            string code,
            string defaultMessage)
        {
            return code switch
            {
                "DuplicateEmail" =>
                    "Email này đã được đăng ký.",

                "DuplicateUserName" =>
                    "Email này đã được đăng ký.",

                "PasswordTooShort" =>
                    "Mật khẩu phải có ít nhất 6 ký tự.",

                "PasswordRequiresDigit" =>
                    "Mật khẩu phải có ít nhất một chữ số.",

                "PasswordRequiresLower" =>
                    "Mật khẩu phải có ít nhất một chữ thường.",

                "PasswordRequiresUpper" =>
                    "Mật khẩu phải có ít nhất một chữ hoa.",

                "PasswordRequiresNonAlphanumeric" =>
                    "Mật khẩu phải có ít nhất một ký tự đặc biệt.",

                _ => defaultMessage
            };
        }
    }
}