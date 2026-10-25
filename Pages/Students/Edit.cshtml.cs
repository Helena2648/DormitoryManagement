
using DormitoryManagement.Data;
using DormitoryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Pages.Students;

public class EditModel : PageModel
{
    private readonly DormitoryDbContext _context;

    public EditModel(DormitoryDbContext context)
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
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var existingStudent = await _context.Students
            .FirstOrDefaultAsync(s => s.StudentId == Student.StudentId);

        if (existingStudent == null)
        {
            return NotFound();
        }

        existingStudent.StudentName = Student.StudentName;
        existingStudent.DateOfBirth = Student.DateOfBirth;
        existingStudent.Gender = Student.Gender;
        existingStudent.Email = Student.Email;
        existingStudent.Phone = Student.Phone;
        existingStudent.Address = Student.Address;

        await _context.SaveChangesAsync();

        return RedirectToPage("/Students/Index");
    }
}
