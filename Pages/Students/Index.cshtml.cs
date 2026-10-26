using DormitoryManagement.Data;
using DormitoryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Pages.Students;

public class IndexModel : PageModel
{
    private readonly DormitoryDbContext _context;

    public IndexModel(DormitoryDbContext context)
    {
        _context = context;
    }

    public List<Student> Students { get; private set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? AssignmentFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? StatusFilter { get; set; } = "Active";

    public string? Message { get; private set; }

    public async Task OnGetAsync()
    {
        var query = _context.Students
            .AsNoTracking()
            .AsQueryable();

        // Filter by active status.
        if (StatusFilter == "Active")
        {
            query = query.Where(s => s.IsActive);
        }
        else if (StatusFilter == "Inactive")
        {
            query = query.Where(s => !s.IsActive);
        }

        // Search by student ID or student name.
        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            var search = SearchTerm.Trim();

            query = query.Where(s =>
                s.StudentId.ToString() == search ||
                s.StudentName.Contains(search));
        }

        // Filter by room assignment status.
        if (AssignmentFilter == "Assigned")
        {
            query = query.Where(s =>
                _context.RoomAssignments.Any(a =>
                    a.StudentId == s.StudentId &&
                    a.Status == "Active"));
        }
        else if (AssignmentFilter == "NotAssigned")
        {
            query = query.Where(s =>
                !_context.RoomAssignments.Any(a =>
                    a.StudentId == s.StudentId &&
                    a.Status == "Active"));
        }

        Students = await query
            .OrderBy(s => s.StudentId)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostToggleStatusAsync(int id)
    {
        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.StudentId == id);

        if (student == null)
        {
            TempData["ErrorMessage"] = "Student not found.";
            return RedirectToPage(new
            {
                SearchTerm,
                AssignmentFilter,
                StatusFilter
            });
        }

        // Prevent deactivation while the student has an active assignment.
        if (student.IsActive)
        {
            var hasActiveAssignment = await _context.RoomAssignments
                .AnyAsync(a =>
                    a.StudentId == student.StudentId &&
                    a.Status == "Active");

            if (hasActiveAssignment)
            {
                TempData["ErrorMessage"] =
                    "Cancel the student's active room assignment before deactivating this student.";
                
                return RedirectToPage(new
                {
                    SearchTerm,
                    AssignmentFilter,
                    StatusFilter
                });
            }
        }

        student.IsActive = !student.IsActive;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = student.IsActive
            ? $"Student {student.StudentName} has been reactivated."
            : $"Student {student.StudentName} has been deactivated.";

        return RedirectToPage(new
        {
            SearchTerm,
            AssignmentFilter,
            StatusFilter
        });
    }
}