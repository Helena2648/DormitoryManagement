using DormitoryManagement.Data;
using DormitoryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Pages.Rooms;

public class EditRoomModel : PageModel
{
    private readonly DormitoryDbContext _context;

    public EditRoomModel(DormitoryDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Room Room { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var room = await _context.Rooms.FindAsync(id);

        if (room == null)
        {
            return NotFound();
        }

        Room = room;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var roomToUpdate = await _context.Rooms.FindAsync(Room.RoomId);

        if (roomToUpdate == null)
        {
            return NotFound();
        }

        roomToUpdate.RoomNumber = Room.RoomNumber;
        roomToUpdate.Floor = Room.Floor;
        roomToUpdate.RoomType = Room.RoomType;
        roomToUpdate.Capacity = Room.Capacity;

        await _context.SaveChangesAsync();

        return RedirectToPage("/Rooms/RoomList");
    }
}