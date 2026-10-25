
using DormitoryManagement.Data;
using DormitoryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Pages.Students;

public class DeleteModel : PageModel
{
    private readonly DormitoryDbContext _context;

    public DeleteModel(DormitoryDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Student Student { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var student = await _context.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.StudentId == id);

        if (student == null)
        {
            return NotFound();
        }

        Student = student;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Clear validation errors from fields not needed for deletion.
        ModelState.Clear();

        var student = await _context.Students
            .FirstOrDefaultAsync(
                s => s.StudentId == Student.StudentId);

        if (student == null)
        {
            return NotFound();
        }

        // Preserve students who have room assignment history.
        bool hasAssignments = await _context.RoomAssignments
            .AnyAsync(a => a.StudentId == student.StudentId);

        if (hasAssignments)
        {
            Student = student;

            ModelState.AddModelError(
                string.Empty,
                "This student has room assignment records. " +
                "To preserve dormitory history, the student cannot be deleted.");

            return Page();
        }

        try
        {
            _context.Students.Remove(student);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Student deleted successfully.";

            return RedirectToPage("/Students/Index");
        }
        catch (DbUpdateException)
        {
            // Handle other database relationships safely.
            Student = student;

            ModelState.AddModelError(
                string.Empty,
                "This student is linked to other records and cannot be deleted.");

            return Page();
        }
    }
}
