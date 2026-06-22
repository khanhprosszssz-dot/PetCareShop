using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetCareShop.Data;

namespace PetCareShop.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser>
            _signInManager;

        private readonly UserManager<ApplicationUser>
            _userManager;

        public LoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lÃ²ng nháº­p email.")]
            [EmailAddress(ErrorMessage = "Email khÃ´ng há»£p lá»‡.")]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lÃ²ng nháº­p máº­t kháº©u.")]
            [DataType(DataType.Password)]
            [Display(Name = "Máº­t kháº©u")]
            public string Password { get; set; } = string.Empty;

            [Display(Name = "Ghi nhá»› Ä‘Äƒng nháº­p")]
            public bool RememberMe { get; set; }
        }

        public IActionResult OnGet(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return LocalRedirect(
                    Url.Content("~/"));
            }

            ReturnUrl = returnUrl ?? Url.Content("~/");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ReturnUrl ??= Url.Content("~/");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            string email = Input.Email.Trim();

            var user =
                await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Email hoáº·c máº­t kháº©u khÃ´ng Ä‘Ãºng.");

                return Page();
            }

            var result =
                await _signInManager.PasswordSignInAsync(
                    user,
                    Input.Password,
                    Input.RememberMe,
                    lockoutOnFailure: true);

            if (result.Succeeded)
            {
                return LocalRedirect(ReturnUrl);
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "TÃ i khoáº£n Ä‘ang bá»‹ khÃ³a táº¡m thá»i do Ä‘Äƒng nháº­p sai quÃ¡ nhiá»u láº§n.");

                return Page();
            }

            if (result.IsNotAllowed)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "TÃ i khoáº£n hiá»‡n chÆ°a Ä‘Æ°á»£c phÃ©p Ä‘Äƒng nháº­p.");

                return Page();
            }

            ModelState.AddModelError(
                string.Empty,
                "Email hoáº·c máº­t kháº©u khÃ´ng Ä‘Ãºng.");

            return Page();
        }
    }
}
