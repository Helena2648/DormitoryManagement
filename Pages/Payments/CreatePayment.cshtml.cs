
using DormitoryManagement.Data;
using DormitoryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Pages.Payments;

public class CreatePaymentModel : PageModel
{
    private readonly DormitoryDbContext _context;
    private readonly ILogger<CreatePaymentModel> _logger;

    public CreatePaymentModel(
        DormitoryDbContext context,
        ILogger<CreatePaymentModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    [BindProperty]
    public Payment Payment { get; set; } = new();

    public List<SelectListItem> RoomOptions { get; set; } = new();

    public async Task OnGetAsync()
    {
        Payment.PaymentDate = DateTime.Today;
        Payment.PaymentStatus = "Unpaid";

        await LoadRoomOptionsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            if (!ModelState.IsValid)
            {
                await LoadRoomOptionsAsync();
                return Page();
            }

            bool roomExists = await _context.Rooms
                .AnyAsync(r => r.RoomId == Payment.RoomId);

            if (!roomExists)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Please select a valid room.");

                await LoadRoomOptionsAsync();
                return Page();
            }

            if (Payment.RentalFee < 0 ||
                Payment.WaterFee < 0 ||
                Payment.ElectricityFee < 0)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Fees cannot be negative.");

                await LoadRoomOptionsAsync();
                return Page();
            }

            _context.Payments.Add(Payment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Payment recorded successfully.";

            return RedirectToPage("/Payments/PaymentList");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save payment.");

            ModelState.AddModelError(
                string.Empty,
                $"Could not save payment: {ex.GetBaseException().Message}");

            await LoadRoomOptionsAsync();
            return Page();
        }
    }

    private async Task LoadRoomOptionsAsync()
    {
        RoomOptions = await _context.Rooms
            .AsNoTracking()
            .OrderBy(r => r.RoomNumber)
            .Select(r => new SelectListItem
            {
                Value = r.RoomId.ToString(),
                Text = r.RoomNumber
            })
            .ToListAsync();
    }
}
