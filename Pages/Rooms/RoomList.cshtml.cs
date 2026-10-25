using DormitoryManagement.Data;
using DormitoryManagement.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Pages.Rooms;

public class RoomListModel : PageModel
{
    private readonly DormitoryDbContext _context;

    public RoomListModel(DormitoryDbContext context)
    {
        _context = context;
    }

    public List<Room> Rooms { get; private set; } = new();

    public async Task OnGetAsync()
    {
        Rooms = await _context.Rooms
            .AsNoTracking()
            .OrderBy(r => r.RoomNumber)
            .ToListAsync();
    }
}