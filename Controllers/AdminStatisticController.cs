using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCareShop.Data;

namespace PetCareShop.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminStatisticController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminStatisticController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            int? month,
            int? year)
        {
            DateTime now = DateTime.Now;

            int selectedMonth =
                month ?? now.Month;

            int selectedYear =
                year ?? now.Year;

            if (selectedMonth < 1 ||
                selectedMonth > 12)
            {
                selectedMonth = now.Month;
            }

            if (selectedYear < 2000 ||
                selectedYear > 2100)
            {
                selectedYear = now.Year;
            }

            DateTime today =
                DateTime.Today;

            DateTime tomorrow =
                today.AddDays(1);

            DateTime firstDayOfMonth =
                new DateTime(
                    selectedYear,
                    selectedMonth,
                    1);

            DateTime firstDayOfNextMonth =
                firstDayOfMonth.AddMonths(1);

            ViewBag.SelectedMonth =
                selectedMonth;

            ViewBag.SelectedYear =
                selectedYear;

            ViewBag.TodayRevenue =
                await _context.Orders
                    .Where(order =>
                        order.Status != "Đã hủy" &&
                        order.OrderDate >= today &&
                        order.OrderDate < tomorrow)
                    .Select(order =>
                        (decimal?)order.TotalAmount)
                    .SumAsync() ?? 0;

            ViewBag.MonthRevenue =
                await _context.Orders
                    .Where(order =>
                        order.Status != "Đã hủy" &&
                        order.OrderDate >=
                        firstDayOfMonth &&
                        order.OrderDate <
                        firstDayOfNextMonth)
                    .Select(order =>
                        (decimal?)order.TotalAmount)
                    .SumAsync() ?? 0;

            ViewBag.TotalCompletedOrders =
                await _context.Orders.CountAsync(
                    order =>
                        order.Status == "Hoàn thành");

            ViewBag.TotalCancelledOrders =
                await _context.Orders.CountAsync(
                    order =>
                        order.Status == "Đã hủy");

            ViewBag.TotalOrdersInMonth =
                await _context.Orders.CountAsync(
                    order =>
                        order.OrderDate >=
                        firstDayOfMonth &&
                        order.OrderDate <
                        firstDayOfNextMonth);

            var ordersInMonth =
                await _context.Orders
                    .Where(order =>
                        order.OrderDate >=
                        firstDayOfMonth &&
                        order.OrderDate <
                        firstDayOfNextMonth)
                    .ToListAsync();

            int daysInMonth =
                DateTime.DaysInMonth(
                    selectedYear,
                    selectedMonth);

            var dailyStats =
                new List<DailyRevenueViewModel>();

            for (int day = 1;
                 day <= daysInMonth;
                 day++)
            {
                DateTime date =
                    new DateTime(
                        selectedYear,
                        selectedMonth,
                        day);

                var ordersOfDay =
                    ordersInMonth
                        .Where(order =>
                            order.OrderDate.Date ==
                            date.Date)
                        .ToList();

                decimal revenue =
                    ordersOfDay
                        .Where(order =>
                            order.Status != "Đã hủy")
                        .Sum(order =>
                            order.TotalAmount);

                dailyStats.Add(
                    new DailyRevenueViewModel
                    {
                        Date = date,
                        Revenue = revenue,
                        OrderCount =
                            ordersOfDay.Count,

                        CancelledCount =
                            ordersOfDay.Count(
                                order =>
                                    order.Status ==
                                    "Đã hủy")
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