
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

            // Confirm that the selected student exists.
            var student = await _context.Students
                .AsNoTracking()
                .FirstOrDefaultAsync(s =>
                    s.StudentId == Assignment.StudentId);

            // Confirm that the selected room exists.
            var room = await _context.Rooms
                .AsNoTracking()
                .FirstOrDefaultAsync(r =>
                    r.RoomId == Assignment.RoomId);

            if (student == null || room == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Please select a valid student and room.");

                await LoadOptionsAsync();
                return Page();
            }

            // Prevent inactive students from receiving new assignments.
            if (!student.IsActive)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "This student is inactive. Reactivate the student before creating a room assignment.");

                await LoadOptionsAsync();
                return Page();
            }

            // Validate the assignment status.
            if (Assignment.Status != "Active" &&
                Assignment.Status != "Cancelled")
            {
                ModelState.AddModelError(
                    nameof(Assignment.Status),
                    "Please select a valid assignment status.");

                await LoadOptionsAsync();
                return Page();
            }

            if (Assignment.Status == "Active")
            {
                // Prevent a student from having multiple active assignments.
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

                // Check whether the room has available capacity.
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

            // Save the new room assignment.
            _context.RoomAssignments.Add(Assignment);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Room assignment saved successfully.";

            return RedirectToPage(
                "/Rooms/RoomAssignments/AssignmentList");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to save room assignment.");

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
        // Include all students, but disable inactive students.
        StudentOptions = await _context.Students
            .AsNoTracking()
            .OrderBy(s => s.StudentName)
            .Select(s => new SelectListItem
            {
                Value = s.StudentId.ToString(),
                Text = s.StudentName +
                       (s.IsActive ? "" : " (Inactive)"),
                Disabled = !s.IsActive
            })
            .ToListAsync();

        // Count active assignments for each room.
        var occupancyByRoom = await _context.RoomAssignments
            .AsNoTracking()
            .Where(a => a.Status == "Active")
            .GroupBy(a => a.RoomId)
            .Select(group => new
            {
                RoomId = group.Key,
                Occupants = group.Count()
            })
            .ToDictionaryAsync(
                group => group.RoomId,
                group => group.Occupants);

        // Load rooms and display their occupancy and availability.
        var rooms = await _context.Rooms
            .AsNoTracking()
            .OrderBy(r => r.RoomNumber)
            .Select(r => new
            {
                r.RoomId,
                r.RoomNumber,
                r.Capacity
            })
            .ToListAsync();

        RoomOptions = rooms.Select(room =>
        {
            int occupants =
                occupancyByRoom.GetValueOrDefault(room.RoomId);

            int availableSpaces =
                Math.Max(0, room.Capacity - occupants);

            string availability = availableSpaces == 0
                ? "FULL"
                : $"{availableSpaces} spaces available";

            return new SelectListItem
            {
                Value = room.RoomId.ToString(),
                Text = $"Room {room.RoomNumber} — " +
                       $"{occupants}/{room.Capacity} occupied, " +
                       availability,
                Disabled = availableSpaces == 0
            };
        }).ToList();
    }
}
