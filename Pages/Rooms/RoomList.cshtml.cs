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

    public Dictionary<int, int> CurrentOccupants { get; private set; } = new();

    public int GetAvailableSpaces(Room room)
    {
        int occupants = CurrentOccupants.GetValueOrDefault(room.RoomId);
        return Math.Max(0, room.Capacity - occupants);
    }

    public async Task OnGetAsync()
    {
        // Load all registered rooms.
        Rooms = await _context.Rooms
            .AsNoTracking()
            .OrderBy(r => r.RoomNumber)
            .ToListAsync();

        // Count students with active assignments in each room.
        CurrentOccupants = await _context.RoomAssignments
            .AsNoTracking()
            .Where(a => a.Status == "Active")
            .GroupBy(a => a.RoomId)
            .Select(group => new
            {
                RoomId = group.Key,
                Occupants = group.Count()
            })
            .ToDictionaryAsync(
                group => group.RoomId,
                group => group.Occupants);
    }
}