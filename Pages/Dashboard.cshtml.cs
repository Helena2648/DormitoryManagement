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

    public int AvailableSpaces { get; private set; }

    public decimal TotalPayments { get; private set; }

    public int UnpaidPayments { get; private set; }

    public decimal UnpaidAmount { get; private set; }

    public async Task OnGetAsync()
    {
        // Count active students.
        TotalStudents = await _context.Students
            .AsNoTracking()
            .CountAsync(s => s.IsActive);

        // Show only active students in the dashboard table.
        Students = await _context.Students
            .AsNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.StudentId)
            .ToListAsync();

        // Load room capacities and active assignment counts.
        var roomOccupancy = await _context.Rooms
            .AsNoTracking()
            .Select(room => new
            {
                room.RoomId,
                room.Capacity,
                ActiveOccupants = _context.RoomAssignments.Count(assignment =>
                    assignment.RoomId == room.RoomId &&
                    assignment.Status == "Active")
            })
            .ToListAsync();

        // Count all registered rooms.
        TotalRooms = roomOccupancy.Count;

        // Count rooms that have reached their capacity.
        OccupiedRooms = roomOccupancy.Count(room =>
            room.Capacity > 0 &&
            room.ActiveOccupants >= room.Capacity);

        // Count rooms with space for at least one more student.
        AvailableRooms = roomOccupancy.Count(room =>
            room.Capacity > 0 &&
            room.ActiveOccupants < room.Capacity);

        // Calculate the total remaining spaces across all rooms.
        AvailableSpaces = roomOccupancy.Sum(room =>
            Math.Max(0, room.Capacity - room.ActiveOccupants));

        // Calculate the total value of all recorded payments.
        TotalPayments = await _context.Payments
            .AsNoTracking()
            .Select(payment =>
                (decimal?)(payment.RentalFee
                           + payment.WaterFee
                           + payment.ElectricityFee))
            .SumAsync() ?? 0m;

        // Count unpaid payment records.
        UnpaidPayments = await _context.Payments
            .AsNoTracking()
            .CountAsync(payment =>
                payment.PaymentStatus == "Unpaid");

        // Calculate the total outstanding amount.
        UnpaidAmount = await _context.Payments
            .AsNoTracking()
            .Where(payment =>
                payment.PaymentStatus == "Unpaid")
            .Select(payment =>
                (decimal?)(payment.RentalFee
                           + payment.WaterFee
                           + payment.ElectricityFee))
            .SumAsync() ?? 0m;
    }
}