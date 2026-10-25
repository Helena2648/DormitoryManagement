using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DormitoryManagement.Models;

[Table("ROOM")]
public class Room
{
    [Key]
    [Column("Room_ID")]
    public int RoomId { get; set; }

    [Required]
    [MaxLength(20)]
    [Column("Room_Number")]
    public string RoomNumber { get; set; } = string.Empty;

    [Column("Floor")]
    public int? Floor { get; set; }

    [MaxLength(50)]
    [Column("Room_Type")]
    public string? RoomType { get; set; }

    [Column("Capacity")]
    public int Capacity { get; set; }
}