using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCareShop.Data;
using PetCareShop.Models;

namespace PetCareShop.Controllers
{
    [Authorize]
    public class CustomerProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public CustomerProfileController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // Hiển thị hồ sơ của tài khoản Identity đang đăng nhập
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var customer = await GetOrCreateCustomerAsync(user);

            if (customer == null)
            {
                TempData["ProfileError"] =
                    "Tài khoản hiện tại chưa có email hợp lệ.";

                return RedirectToAction("Index", "Home");
            }

            return View(customer);
        }

        // Cập nhật họ tên, số điện thoại và địa chỉ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateInfo(
            string FullName,
            string Phone,
            string Address)
        {
            if (string.IsNullOrWhiteSpace(FullName) ||
                string.IsNullOrWhiteSpace(Phone) ||
                string.IsNullOrWhiteSpace(Address))
            {
                TempData["ProfileError"] =
                    "Vui lòng nhập đầy đủ họ tên, số điện thoại và địa chỉ.";

                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            // Cập nhật thông tin trong bảng AspNetUsers
            user.FullName = FullName.Trim();
            user.PhoneNumber = Phone.Trim();
            user.Address = Address.Trim();

            var updateUserResult =
                await _userManager.UpdateAsync(user);

            if (!updateUserResult.Succeeded)
            {
                TempData["ProfileError"] = string.Join(
                    " ",
                    updateUserResult.Errors.Select(error =>
                        error.Description));

                return RedirectToAction(nameof(Index));
            }

            // Đồng bộ dữ liệu sang bảng Customers hiện tại
            var customer = await GetOrCreateCustomerAsync(user);

            if (customer == null)
            {
                TempData["ProfileError"] =
                    "Không thể tìm thấy hồ sơ khách hàng.";

                return RedirectToAction(nameof(Index));
            }

            customer.FullName = user.FullName;
            customer.Phone = user.PhoneNumber ?? string.Empty;
            customer.Address = user.Address;

            await _context.SaveChangesAsync();

            // Làm mới cookie đăng nhập để nhận thông tin mới
            await _signInManager.RefreshSignInAsync(user);

            TempData["ProfileSuccess"] =
                "Cập nhật thông tin thành công.";

            return RedirectToAction(nameof(Index));
        }

        // Đổi mật khẩu bằng ASP.NET Core Identity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(
            string OldPassword,
            string NewPassword,
            string ConfirmPassword)
        {
            if (string.IsNullOrWhiteSpace(OldPassword) ||
                string.IsNullOrWhiteSpace(NewPassword) ||
                string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                TempData["PasswordError"] =
                    "Vui lòng nhập đầy đủ thông tin đổi mật khẩu.";

                return RedirectToAction(nameof(Index));
            }

            if (NewPassword.Length < 6)
            {
                TempData["PasswordError"] =
                    "Mật khẩu mới phải có ít nhất 6 ký tự.";

                return RedirectToAction(nameof(Index));
            }

            if (NewPassword != ConfirmPassword)
            {
                TempData["PasswordError"] =
                    "Xác nhận mật khẩu mới không khớp.";

                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var changePasswordResult =
                await _userManager.ChangePasswordAsync(
                    user,
                    OldPassword,
                    NewPassword);

            if (!changePasswordResult.Succeeded)
            {
                string errorMessage = string.Join(
                    " ",
                    changePasswordResult.Errors.Select(error =>
                        TranslateIdentityError(error.Code)));

                TempData["PasswordError"] =
                    string.IsNullOrWhiteSpace(errorMessage)
                        ? "Mật khẩu cũ không đúng hoặc mật khẩu mới không hợp lệ."
                        : errorMessage;

                return RedirectToAction(nameof(Index));
            }

            // Làm mới cookie sau khi thay đổi mật khẩu
            await _signInManager.RefreshSignInAsync(user);

            TempData["PasswordSuccess"] =
                "Đổi mật khẩu thành công.";

            return RedirectToAction(nameof(Index));
        }

        // Tìm hồ sơ Customer theo email Identity.
        // Nếu chưa có thì tạo mới để các chức năng cũ vẫn hoạt động.
        private async Task<Customer?> GetOrCreateCustomerAsync(
            ApplicationUser user)
        {
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                return null;
            }

            string email = user.Email.Trim();

            var customer = await _context.Customers
                .FirstOrDefaultAsync(item =>
                    item.Email == email);

            if (customer != null)
            {
                return customer;
            }

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

            return customer;
        }

        private static string TranslateIdentityError(
            string errorCode)
        {
            return errorCode switch
            {
                "PasswordMismatch" =>
                    "Mật khẩu cũ không đúng.",

                "PasswordTooShort" =>
                    "Mật khẩu mới quá ngắn.",

                "PasswordRequiresDigit" =>
                    "Mật khẩu mới phải có ít nhất một chữ số.",

                "PasswordRequiresLower" =>
                    "Mật khẩu mới phải có ít nhất một chữ thường.",

                "PasswordRequiresUpper" =>
                    "Mật khẩu mới phải có ít nhất một chữ hoa.",

                "PasswordRequiresNonAlphanumeric" =>
                    "Mật khẩu mới phải có ít nhất một ký tự đặc biệt.",

                _ =>
                    "Mật khẩu mới không hợp lệ."
            };
        }
    }
}
