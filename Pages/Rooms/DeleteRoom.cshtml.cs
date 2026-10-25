using DormitoryManagement.Data;
using DormitoryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Pages.Rooms;

public class DeleteRoomModel : PageModel
{
    private readonly DormitoryDbContext _context;

    public DeleteRoomModel(DormitoryDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Room Room { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var room = await _context.Rooms
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.RoomId == id);

        if (room == null)
        {
            return NotFound();
        }

        Room = room;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var room = await _context.Rooms.FindAsync(Room.RoomId);

        if (room == null)
        {
            return NotFound();
        }

        _context.Rooms.Remove(room);
        await _context.SaveChangesAsync();

        return RedirectToPage("/Rooms/RoomList");
    }
}