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

    public async Task OnGetAsync()
    {
        var query = _context.Students
            .AsNoTracking()
            .AsQueryable();

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
}