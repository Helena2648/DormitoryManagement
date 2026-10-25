
using DormitoryManagement.Data;
using DormitoryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Pages.Payments;

public class DeletePaymentModel : PageModel
{
    private readonly DormitoryDbContext _context;
    private readonly ILogger<DeletePaymentModel> _logger;

    public DeletePaymentModel(
        DormitoryDbContext context,
        ILogger<DeletePaymentModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    public Payment? Payment { get; private set; }

    public decimal Total =>
        Payment == null
            ? 0
            : Payment.RentalFee
              + Payment.WaterFee
              + Payment.ElectricityFee;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Payment = await _context.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PaymentId == id);

        if (Payment == null)
            return NotFound();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        try
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.PaymentId == id);

            if (payment == null)
            {
                TempData["ErrorMessage"] =
                    "Payment not found. It may already have been deleted.";

                return RedirectToPage("/Payments/PaymentList");
            }

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"Payment #{payment.PaymentId} was deleted successfully.";

            return RedirectToPage("/Payments/PaymentList");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex, "Failed to delete payment {PaymentId}", id);

            TempData["ErrorMessage"] =
                "Could not delete this payment. " +
                ex.GetBaseException().Message;

            return RedirectToPage("/Payments/PaymentList");
        }
    }
}
