
using System.ComponentModel.DataAnnotations;
using DormitoryManagement.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Pages.Rooms.RoomAssignments;

public class EditAssignmentModel : PageModel
{
    private readonly DormitoryDbContext _context;
    private readonly ILogger<EditAssignmentModel> _logger;

    public EditAssignmentModel(
        DormitoryDbContext context,
        ILogger<EditAssignmentModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    [BindProperty]
    public AssignmentInput Input { get; set; } = new();

    public List<SelectListItem> StudentOptions { get; set; } = new();
    public List<SelectListItem> RoomOptions { get; set; } = new();

    public class AssignmentInput
    {
        public int AssignmentId { get; set; }

        [Required(ErrorMessage = "Please select a student.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid student.")]
        public int StudentId { get; set; }

        [Required(ErrorMessage = "Please select a room.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid room.")]
        public int RoomId { get; set; }

        [Required(ErrorMessage = "Check-in date is required.")]
        [DataType(DataType.Date)]
        public DateTime CheckInDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? CheckOutDate { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Active";

        [StringLength(255)]
        public string? Notes { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var assignment = await _context.RoomAssignments
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.AssignmentId == id);

        if (assignment == null)
            return NotFound();

        Input = new AssignmentInput
        {
            AssignmentId = assignment.AssignmentId,
            StudentId = assignment.StudentId,
            RoomId = assignment.RoomId,
            CheckInDate = assignment.CheckInDate,
            CheckOutDate = assignment.CheckOutDate,
            Status = assignment.Status,
            Notes = assignment.Notes
        };

        await LoadOptionsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            var assignment = await _context.RoomAssignments
                .FirstOrDefaultAsync(a =>
                    a.AssignmentId == Input.AssignmentId);

            if (assignment == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                await LoadOptionsAsync();
                return Page();
            }

            if (Input.CheckOutDate.HasValue &&
                Input.CheckOutDate.Value.Date < Input.CheckInDate.Date)
            {
                ModelState.AddModelError(
                    "Input.CheckOutDate",
                    "Check-out date cannot be earlier than check-in date.");

                await LoadOptionsAsync();
                return Page();
            }

            bool studentExists = await _context.Students
                .AnyAsync(s => s.StudentId == Input.StudentId);

            var room = await _context.Rooms
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RoomId == Input.RoomId);

            if (!studentExists || room == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Please select a valid student and room.");

                await LoadOptionsAsync();
                return Page();
            }

            string[] allowedStatuses = { "Active", "Completed", "Cancelled" };

            if (!allowedStatuses.Contains(Input.Status))
            {
                ModelState.AddModelError(
                    "Input.Status",
                    "Please select a valid assignment status.");

                await LoadOptionsAsync();
                return Page();
            }

            if (Input.Status == "Active")
            {
                bool studentHasAnotherActiveAssignment =
                    await _context.RoomAssignments.AnyAsync(a =>
                        a.StudentId == Input.StudentId &&
                        a.AssignmentId != Input.AssignmentId &&
                        a.Status == "Active");

                if (studentHasAnotherActiveAssignment)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "This student already has another active room assignment.");

                    await LoadOptionsAsync();
                    return Page();
                }

                int activeOccupants = await _context.RoomAssignments
                    .CountAsync(a =>
                        a.RoomId == Input.RoomId &&
                        a.AssignmentId != Input.AssignmentId &&
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

            assignment.StudentId = Input.StudentId;
            assignment.RoomId = Input.RoomId;
            assignment.CheckInDate = Input.CheckInDate.Date;
            assignment.CheckOutDate = Input.CheckOutDate?.Date;
            assignment.Status = Input.Status;
            assignment.Notes = Input.Notes;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Room assignment updated successfully.";

            return RedirectToPage(
                "/Rooms/RoomAssignments/AssignmentList");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update room assignment.");

            ModelState.AddModelError(
                string.Empty,
                "Could not update the assignment: " +
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
