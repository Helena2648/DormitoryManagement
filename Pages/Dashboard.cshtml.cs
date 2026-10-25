using DormitoryManagement.Data;
using DormitoryManagement.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Pages;

public class DashboardModel : PageModel
{
    private readonly DormitoryDbContext _context;

    public DashboardModel(DormitoryDbContext context)
    {
        _context = context;
    }

    public List<Student> Students { get; private set; } = new();

    public int TotalStudents { get; private set; }

    public int TotalRooms { get; private set; }

    public int OccupiedRooms { get; private set; }

    public int AvailableRooms { get; private set; }

    public decimal TotalPayments { get; private set; }

    public async Task OnGetAsync()
    {
        // Load students for the existing student list.
        Students = await _context.Students
            .AsNoTracking()
            .OrderBy(s => s.StudentId)
            .ToListAsync();

        TotalStudents = Students.Count;

        // Count all rooms.
        TotalRooms = await _context.Rooms
            .AsNoTracking()
            .CountAsync();

        // Count rooms with at least one active assignment.
        OccupiedRooms = await _context.Rooms
            .AsNoTracking()
            .CountAsync(room =>
                _context.RoomAssignments.Any(assignment =>
                    assignment.RoomId == room.RoomId &&
                    assignment.Status == "Active"));

        // Available rooms means rooms without an active assignment.
        AvailableRooms = TotalRooms - OccupiedRooms;

        // Sum recorded payments, including all payment statuses.
        TotalPayments = await _context.Payments
            .AsNoTracking()
            .Select(payment =>
                (decimal?)(payment.RentalFee
                           + payment.WaterFee
                           + payment.ElectricityFee))
            .SumAsync() ?? 0m;
    }
}