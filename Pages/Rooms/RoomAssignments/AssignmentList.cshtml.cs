using DormitoryManagement.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Pages.Rooms.RoomAssignments;

public class AssignmentListModel : PageModel
{
    private readonly DormitoryDbContext _context;

    public AssignmentListModel(DormitoryDbContext context)
    {
        _context = context;
    }

    public List<AssignmentRow> Assignments { get; private set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? StatusFilter { get; set; } = "All";

    public async Task OnGetAsync()
    {
        await LoadAssignmentsAsync();
    }

    public async Task<IActionResult> OnPostCancelAsync(int id)
    {
        var assignment = await _context.RoomAssignments
            .FirstOrDefaultAsync(a => a.AssignmentId == id);

        if (assignment == null)
        {
            TempData["ErrorMessage"] = "Assignment not found.";
            return RedirectToPage();
        }

        if (assignment.Status != "Active")
        {
            TempData["ErrorMessage"] =
                "Only active assignments can be cancelled.";

            return RedirectToPage();
        }

        assignment.Status = "Cancelled";

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Room assignment cancelled successfully.";

        return RedirectToPage();
    }

    private async Task LoadAssignmentsAsync()
    {
        var query =
            from assignment in _context.RoomAssignments.AsNoTracking()
            join student in _context.Students.AsNoTracking()
                on assignment.StudentId equals student.StudentId
            join room in _context.Rooms.AsNoTracking()
                on assignment.RoomId equals room.RoomId
            select new AssignmentRow
            {
                AssignmentId = assignment.AssignmentId,
                StudentId = student.StudentId,
                StudentName = student.StudentName,
                RoomId = room.RoomId,
                RoomNumber = room.RoomNumber,
                CheckInDate = assignment.CheckInDate,
                CheckOutDate = assignment.CheckOutDate,
                Status = assignment.Status
            };

        // Search by assignment ID, student ID, student name, or room number.
        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            string search = SearchTerm.Trim();

            query = query.Where(a =>
                a.StudentName.Contains(search) ||
                a.RoomNumber.Contains(search) ||
                a.AssignmentId.ToString() == search ||
                a.StudentId.ToString() == search);
        }

        // Filter by assignment status.
        if (!string.IsNullOrWhiteSpace(StatusFilter) &&
            StatusFilter != "All")
        {
            if (StatusFilter == "Active" ||
                StatusFilter == "Completed" ||
                StatusFilter == "Cancelled")
            {
                query = query.Where(a => a.Status == StatusFilter);
            }
            else
            {
                // Ignore unsupported status values.
                StatusFilter = "All";
            }
        }
        else
        {
            StatusFilter = "All";
        }

        Assignments = await query
            .OrderByDescending(a => a.AssignmentId)
            .ToListAsync();
    }

    public class AssignmentRow
    {
        public int AssignmentId { get; set; }

        public int StudentId { get; set; }

        public string StudentName { get; set; } = string.Empty;

        public int RoomId { get; set; }

        public string RoomNumber { get; set; } = string.Empty;

        public DateTime CheckInDate { get; set; }

        public DateTime? CheckOutDate { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}