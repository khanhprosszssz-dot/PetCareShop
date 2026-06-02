using Microsoft.AspNetCore.Mvc;
using PetCareShop.Data;

namespace PetCareShop.Controllers
{
    public class CustomerProfileController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsCustomerLoggedIn()
        {
            return HttpContext.Session.GetString("CustomerLogin") == "true";
        }

        public async Task<IActionResult> Index()
        {
            if (!IsCustomerLoggedIn())
            {
                return RedirectToAction("Login", "CustomerAccount");
            }

            var customerId = HttpContext.Session.GetInt32("CustomerId");

            if (customerId == null)
            {
                return RedirectToAction("Login", "CustomerAccount");
            }

            var customer = await _context.Customers.FindAsync(customerId.Value);

            if (customer == null)
            {
                HttpContext.Session.Remove("CustomerLogin");
                HttpContext.Session.Remove("CustomerId");
                HttpContext.Session.Remove("CustomerName");

                return RedirectToAction("Login", "CustomerAccount");
            }

            return View(customer);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateInfo(
            string FullName,
            string Phone,
            string Address)
        {
            if (!IsCustomerLoggedIn())
            {
                return RedirectToAction("Login", "CustomerAccount");
            }

            var customerId = HttpContext.Session.GetInt32("CustomerId");

            if (customerId == null)
            {
                return RedirectToAction("Login", "CustomerAccount");
            }

            var customer = await _context.Customers.FindAsync(customerId.Value);

            if (customer == null)
            {
                return RedirectToAction("Login", "CustomerAccount");
            }

            if (string.IsNullOrWhiteSpace(FullName) ||
                string.IsNullOrWhiteSpace(Phone) ||
                string.IsNullOrWhiteSpace(Address))
            {
                TempData["ProfileError"] = "Vui lòng nhập đầy đủ họ tên, số điện thoại và địa chỉ.";
                return RedirectToAction("Index");
            }

            customer.FullName = FullName;
            customer.Phone = Phone;
            customer.Address = Address;

            await _context.SaveChangesAsync();

            HttpContext.Session.SetString("CustomerName", customer.FullName);

            TempData["ProfileSuccess"] = "Cập nhật thông tin thành công.";

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(
            string OldPassword,
            string NewPassword,
            string ConfirmPassword)
        {
            if (!IsCustomerLoggedIn())
            {
                return RedirectToAction("Login", "CustomerAccount");
            }

            var customerId = HttpContext.Session.GetInt32("CustomerId");

            if (customerId == null)
            {
                return RedirectToAction("Login", "CustomerAccount");
            }

            var customer = await _context.Customers.FindAsync(customerId.Value);

            if (customer == null)
            {
                return RedirectToAction("Login", "CustomerAccount");
            }

            if (string.IsNullOrWhiteSpace(OldPassword) ||
                string.IsNullOrWhiteSpace(NewPassword) ||
                string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                TempData["PasswordError"] = "Vui lòng nhập đầy đủ thông tin đổi mật khẩu.";
                return RedirectToAction("Index");
            }

            if (customer.Password != OldPassword)
            {
                TempData["PasswordError"] = "Mật khẩu cũ không đúng.";
                return RedirectToAction("Index");
            }

            if (NewPassword.Length < 6)
            {
                TempData["PasswordError"] = "Mật khẩu mới phải có ít nhất 6 ký tự.";
                return RedirectToAction("Index");
            }

            if (NewPassword != ConfirmPassword)
            {
                TempData["PasswordError"] = "Xác nhận mật khẩu mới không khớp.";
                return RedirectToAction("Index");
            }

            customer.Password = NewPassword;

            await _context.SaveChangesAsync();

            TempData["PasswordSuccess"] = "Đổi mật khẩu thành công.";

            return RedirectToAction("Index");
        }
    }
}