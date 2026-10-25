using DormitoryManagement.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Pages.Payments;

public class PaymentReportsModel : PageModel
{
    private readonly DormitoryDbContext _context;

    public PaymentReportsModel(DormitoryDbContext context)
    {
        _context = context;
    }

    public decimal TotalCollected { get; private set; }

    public decimal OutstandingPayments { get; private set; }

    public int TotalPaymentRecords { get; private set; }

    public int PaidCount { get; private set; }

    public int PendingCount { get; private set; }

    public int OverdueCount { get; private set; }

    public int UnpaidCount { get; private set; }

    public List<MonthlyRevenueRow> MonthlyRevenue { get; private set; } = new();

    public async Task OnGetAsync()
    {
        var payments = _context.Payments.AsNoTracking();

        TotalPaymentRecords = await payments.CountAsync();

        TotalCollected = await payments
            .Where(p => p.PaymentStatus == "Paid")
            .Select(p => (decimal?)(
                p.RentalFee + p.WaterFee + p.ElectricityFee))
            .SumAsync() ?? 0m;

        OutstandingPayments = await payments
            .Where(p => p.PaymentStatus != "Paid")
            .Select(p => (decimal?)(
                p.RentalFee + p.WaterFee + p.ElectricityFee))
            .SumAsync() ?? 0m;

        PaidCount = await payments
            .CountAsync(p => p.PaymentStatus == "Paid");

        PendingCount = await payments
            .CountAsync(p => p.PaymentStatus == "Pending");

        OverdueCount = await payments
            .CountAsync(p => p.PaymentStatus == "Overdue");

        UnpaidCount = await payments
            .CountAsync(p => p.PaymentStatus == "Unpaid");

        MonthlyRevenue = await payments
            .Where(p => p.PaymentStatus == "Paid")
            .GroupBy(p => new
            {
                p.PaymentDate.Year,
                p.PaymentDate.Month
            })
            .Select(group => new MonthlyRevenueRow
            {
                Year = group.Key.Year,
                Month = group.Key.Month,
                Amount = group.Sum(p =>
                    p.RentalFee + p.WaterFee + p.ElectricityFee),
                PaymentCount = group.Count()
            })
            .OrderByDescending(row => row.Year)
            .ThenByDescending(row => row.Month)
            .ToListAsync();
    }

    public class MonthlyRevenueRow
    {
        public int Year { get; set; }

        public int Month { get; set; }

        public decimal Amount { get; set; }

        public int PaymentCount { get; set; }

        public string MonthName =>
            new DateTime(Year, Month, 1).ToString("MMMM yyyy");
    }
}