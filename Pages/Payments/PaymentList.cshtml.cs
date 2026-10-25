using DormitoryManagement.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Pages.Payments;

public class PaymentListModel : PageModel
{
    private readonly DormitoryDbContext _context;

    public PaymentListModel(DormitoryDbContext context)
    {
        _context = context;
    }

    public List<PaymentRow> Payments { get; private set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? StatusFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? StartDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? EndDate { get; set; }

    public string? FilterError { get; private set; }

    public async Task OnGetAsync()
    {
        if (StartDate.HasValue &&
            EndDate.HasValue &&
            StartDate.Value.Date > EndDate.Value.Date)
        {
            FilterError = "Start date cannot be later than end date.";
            return;
        }

        var query =
            from payment in _context.Payments.AsNoTracking()
            join room in _context.Rooms.AsNoTracking()
                on payment.RoomId equals room.RoomId
            select new PaymentRow
            {
                PaymentId = payment.PaymentId,
                RoomId = payment.RoomId,
                RoomNumber = room.RoomNumber,
                RentalFee = payment.RentalFee,
                WaterFee = payment.WaterFee,
                ElectricityFee = payment.ElectricityFee,
                PaymentMethod = payment.PaymentMethod,
                PaymentDate = payment.PaymentDate,
                PaymentStatus = payment.PaymentStatus,
                Notes = payment.Notes
            };

        // Search by payment ID or room number.
        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            var search = SearchTerm.Trim();

            query = query.Where(p =>
                p.RoomNumber.Contains(search) ||
                p.PaymentId.ToString() == search);
        }

        // Filter by payment status.
        if (!string.IsNullOrWhiteSpace(StatusFilter))
        {
            query = query.Where(p =>
                p.PaymentStatus == StatusFilter);
        }

        // Filter by start date.
        if (StartDate.HasValue)
        {
            var start = StartDate.Value.Date;
            query = query.Where(p => p.PaymentDate >= start);
        }

        // Filter by end date, including the entire selected day.
        if (EndDate.HasValue)
        {
            var endExclusive = EndDate.Value.Date.AddDays(1);
            query = query.Where(p => p.PaymentDate < endExclusive);
        }

        Payments = await query
            .OrderByDescending(p => p.PaymentDate)
            .ThenByDescending(p => p.PaymentId)
            .ToListAsync();
    }

    public class PaymentRow
    {
        public int PaymentId { get; set; }

        public int RoomId { get; set; }

        public string RoomNumber { get; set; } = string.Empty;

        public decimal RentalFee { get; set; }

        public decimal WaterFee { get; set; }

        public decimal ElectricityFee { get; set; }

        public string? PaymentMethod { get; set; }

        public DateTime PaymentDate { get; set; }

        public string PaymentStatus { get; set; } = string.Empty;

        public string? Notes { get; set; }

        public decimal Total =>
            RentalFee + WaterFee + ElectricityFee;
    }
}