using DormitoryManagement.Data;
using DormitoryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Pages.Students;

public class CreateModel : PageModel
{
    private readonly DormitoryDbContext _context;

    public CreateModel(DormitoryDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Student Student { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Normalize the submitted name.
        string studentName = Student.StudentName.Trim();

        // Check whether the name already exists.
        bool nameExists = await _context.Students.AnyAsync(s =>
            s.StudentName.Trim().ToLower() == studentName.ToLower());

        if (nameExists)
        {
            ModelState.AddModelError(
                "Student.StudentName",
                "This student name already exists. Please use a different name.");

            return Page();
        }

        // Save the student with the trimmed name.
        Student.StudentName = studentName;

        _context.Students.Add(Student);
        await _context.SaveChangesAsync();

        return RedirectToPage("/Students/Index");
    }
}