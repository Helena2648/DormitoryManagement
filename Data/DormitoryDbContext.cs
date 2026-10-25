using DormitoryManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Data;

public class DormitoryDbContext : DbContext
{
    public DormitoryDbContext(
        DbContextOptions<DormitoryDbContext> options)
        : base(options)
    {
    }

    public DbSet<Student> Students => Set<Student>();

    public DbSet<Room> Rooms => Set<Room>();

    public DbSet<RoomAssignment> RoomAssignments => Set<RoomAssignment>();

    public DbSet<Payment> Payments => Set<Payment>();
}