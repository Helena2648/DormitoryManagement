using DormitoryManagement.Data;
using DormitoryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DormitoryManagement.Pages.Rooms;

public class CreateRoomModel : PageModel
{
    private readonly DormitoryDbContext _context;

    public CreateRoomModel(DormitoryDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Room Room { get; set; } = new();

    public void OnGet()
    {
        Room.Capacity = 1;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        _context.Rooms.Add(Room);
        await _context.SaveChangesAsync();

        return RedirectToPage("/Rooms/RoomList");
    }
}