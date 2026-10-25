
using DormitoryManagement.Data;
using DormitoryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Pages.Payments;

public class EditPaymentModel : PageModel
{
    private readonly DormitoryDbContext _context;

    public EditPaymentModel(DormitoryDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Payment Payment { get; set; } = new();

    public List<SelectListItem> RoomOptions { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var payment = await _context.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PaymentId == id);

        if (payment == null)
            return NotFound();

        Payment = payment;
        await LoadRoomOptionsAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadRoomOptionsAsync();
            return Page();
        }

        var existingPayment = await _context.Payments
            .FirstOrDefaultAsync(p => p.PaymentId == Payment.PaymentId);

        if (existingPayment == null)
            return NotFound();

        bool roomExists = await _context.Rooms
            .AnyAsync(r => r.RoomId == Payment.RoomId);

        if (!roomExists)
        {
            ModelState.AddModelError(
                string.Empty, "Please select a valid room.");

            await LoadRoomOptionsAsync();
            return Page();
        }

        existingPayment.RoomId = Payment.RoomId;
        existingPayment.RentalFee = Payment.RentalFee;
        existingPayment.WaterFee = Payment.WaterFee;
        existingPayment.ElectricityFee = Payment.ElectricityFee;
        existingPayment.PaymentMethod = Payment.PaymentMethod;
        existingPayment.PaymentDate = Payment.PaymentDate;
        existingPayment.PaymentStatus = Payment.PaymentStatus;
        existingPayment.Notes = Payment.Notes;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Payment updated successfully.";

        return RedirectToPage("/Payments/PaymentList");
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
