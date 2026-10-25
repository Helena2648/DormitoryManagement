using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace DormitoryManagement.Models;

[Table("ROOM_ASSIGNMENT")]
public class RoomAssignment
{
    [Key]
    [Column("Assignment_ID")]
    public int AssignmentId { get; set; }

    [Column("Room_ID")]
    public int RoomId { get; set; }

    [Column("Student_ID")]
    public int StudentId { get; set; }

    [Column("Check_In_Date", TypeName = "date")]
    public DateTime CheckInDate { get; set; }

    [Column("Check_Out_Date", TypeName = "date")]
    public DateTime? CheckOutDate { get; set; }

    [Required]
    [MaxLength(30)]
    [Column("Status")]
    public string Status { get; set; } = "Active";

    [MaxLength(255)]
    [Column("Notes")]
    public string? Notes { get; set; }

    [ValidateNever]
    [ForeignKey(nameof(RoomId))]
    public Room Room { get; set; } = null!;

    [ValidateNever]
    [ForeignKey(nameof(StudentId))]
    public Student Student { get; set; } = null!;
}