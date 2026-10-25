
using DormitoryManagement.Data;
using DormitoryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Pages.Rooms.RoomAssignments;

public class CreateAssignmentModel : PageModel
{
    private readonly DormitoryDbContext _context;
    private readonly ILogger<CreateAssignmentModel> _logger;

    public CreateAssignmentModel(
        DormitoryDbContext context,
        ILogger<CreateAssignmentModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    [BindProperty]
    public RoomAssignment Assignment { get; set; } = new();

    public List<SelectListItem> StudentOptions { get; set; } = new();
    public List<SelectListItem> RoomOptions { get; set; } = new();

    public async Task OnGetAsync()
    {
        Assignment.CheckInDate = DateTime.Today;
        Assignment.Status = "Active";

        await LoadOptionsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            if (!ModelState.IsValid)
            {
                await LoadOptionsAsync();
                return Page();
            }

            bool studentExists = await _context.Students
                .AnyAsync(s => s.StudentId == Assignment.StudentId);

            var room = await _context.Rooms
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RoomId == Assignment.RoomId);

            if (!studentExists || room == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Please select a valid student and room.");

                await LoadOptionsAsync();
                return Page();
            }

            if (Assignment.Status == "Active")
            {
                bool studentHasActiveAssignment =
                    await _context.RoomAssignments.AnyAsync(a =>
                        a.StudentId == Assignment.StudentId &&
                        a.Status == "Active");

                if (studentHasActiveAssignment)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "This student already has an active room assignment. " +
                        "Complete or cancel the existing assignment first.");

                    await LoadOptionsAsync();
                    return Page();
                }

                int activeOccupants = await _context.RoomAssignments
                    .CountAsync(a =>
                        a.RoomId == Assignment.RoomId &&
                        a.Status == "Active");

                if (activeOccupants >= room.Capacity)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        $"Room {room.RoomNumber} is full. " +
                        $"Its capacity is {room.Capacity}.");

                    await LoadOptionsAsync();
                    return Page();
                }
            }

            _context.RoomAssignments.Add(Assignment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Room assignment saved successfully.";

            return RedirectToPage(
                "/Rooms/RoomAssignments/AssignmentList");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save room assignment.");

            ModelState.AddModelError(
                string.Empty,
                "Could not save the assignment: " +
                ex.GetBaseException().Message);

            await LoadOptionsAsync();
            return Page();
        }
    }

    private async Task LoadOptionsAsync()
    {
        StudentOptions = await _context.Students
            .AsNoTracking()
            .OrderBy(s => s.StudentName)
            .Select(s => new SelectListItem
            {
                Value = s.StudentId.ToString(),
                Text = s.StudentName
            })
            .ToListAsync();

        RoomOptions = await _context.Rooms
            .AsNoTracking()
            .OrderBy(r => r.RoomNumber)
            .Select(r => new SelectListItem
            {
                Value = r.RoomId.ToString(),
                Text = r.RoomNumber + " (Capacity: " +
                       r.Capacity.ToString() + ")"
            })
            .ToListAsync();
    }
}
