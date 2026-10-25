
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DormitoryManagement.Models;

[Table("STUDENT")]
public class Student
{
    [Key]
    [Column("Student_ID")]
    public int StudentId { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("Student_Name")]
    public string StudentName { get; set; } = string.Empty;

    [Column("Date_of_Birth", TypeName = "date")]
    public DateTime? DateOfBirth { get; set; }

    [MaxLength(20)]
    [Column("Gender")]
    public string? Gender { get; set; }

    [MaxLength(20)]
    [Column("Phone")]
    public string? Phone { get; set; }

    [MaxLength(100)]
    [Column("Email")]
    public string? Email { get; set; }

    [MaxLength(255)]
    [Column("Address")]
    public string? Address { get; set; }

    [Column("IsActive")]
    public bool IsActive { get; set; } = true;
}
