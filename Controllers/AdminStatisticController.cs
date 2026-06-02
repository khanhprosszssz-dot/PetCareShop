using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCareShop.Data;

namespace PetCareShop.Controllers
{
    public class AdminStatisticController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminStatisticController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsAdminLoggedIn()
        {
            return HttpContext.Session.GetString("AdminLogin") == "true";
        }

        public async Task<IActionResult> Index(int? month, int? year)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "AdminAccount");
            }

            var now = DateTime.Now;

            int selectedMonth = month ?? now.Month;
            int selectedYear = year ?? now.Year;

            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var firstDayOfMonth = new DateTime(selectedYear, selectedMonth, 1);
            var firstDayOfNextMonth = firstDayOfMonth.AddMonths(1);

            ViewBag.SelectedMonth = selectedMonth;
            ViewBag.SelectedYear = selectedYear;

            ViewBag.TodayRevenue = await _context.Orders
                .Where(x => x.Status != "Đã hủy"
                    && x.OrderDate >= today
                    && x.OrderDate < tomorrow)
                .Select(x => (decimal?)x.TotalAmount)
                .SumAsync() ?? 0;

            ViewBag.MonthRevenue = await _context.Orders
                .Where(x => x.Status != "Đã hủy"
                    && x.OrderDate >= firstDayOfMonth
                    && x.OrderDate < firstDayOfNextMonth)
                .Select(x => (decimal?)x.TotalAmount)
                .SumAsync() ?? 0;

            ViewBag.TotalCompletedOrders = await _context.Orders
                .CountAsync(x => x.Status == "Hoàn thành");

            ViewBag.TotalCancelledOrders = await _context.Orders
                .CountAsync(x => x.Status == "Đã hủy");

            ViewBag.TotalOrdersInMonth = await _context.Orders
                .CountAsync(x => x.OrderDate >= firstDayOfMonth
                    && x.OrderDate < firstDayOfNextMonth);

            var ordersInMonth = await _context.Orders
                .Where(x => x.OrderDate >= firstDayOfMonth
                    && x.OrderDate < firstDayOfNextMonth)
                .ToListAsync();

            var daysInMonth = DateTime.DaysInMonth(selectedYear, selectedMonth);

            var dailyStats = new List<DailyRevenueViewModel>();

            for (int day = 1; day <= daysInMonth; day++)
            {
                var date = new DateTime(selectedYear, selectedMonth, day);

                var ordersOfDay = ordersInMonth
                    .Where(x => x.OrderDate.Date == date.Date)
                    .ToList();

                var revenue = ordersOfDay
                    .Where(x => x.Status != "Đã hủy")
                    .Sum(x => x.TotalAmount);

                var orderCount = ordersOfDay.Count;

                var cancelledCount = ordersOfDay.Count(x => x.Status == "Đã hủy");

                dailyStats.Add(new DailyRevenueViewModel
                {
                    Date = date,
                    Revenue = revenue,
                    OrderCount = orderCount,
                    CancelledCount = cancelledCount
                });
            }

            return View(dailyStats);
        }
    }

    public class DailyRevenueViewModel
    {
        public DateTime Date { get; set; }

        public decimal Revenue { get; set; }

        public int OrderCount { get; set; }

        public int CancelledCount { get; set; }
    }
}