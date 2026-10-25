using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DormitoryManagement.Models;

[Table("PAYMENT")]
public class Payment
{
    [Key]
    [Column("Payment_ID")]
    public int PaymentId { get; set; }

    [Column("Room_ID")]
    public int RoomId { get; set; }

    [Column("Rental_Fee", TypeName = "decimal(10,2)")]
    public decimal RentalFee { get; set; }

    [Column("Water_Fee", TypeName = "decimal(10,2)")]
    public decimal WaterFee { get; set; }

    [Column("Electricity_Fee", TypeName = "decimal(10,2)")]
    public decimal ElectricityFee { get; set; }

    [MaxLength(50)]
    [Column("Payment_Method")]
    public string? PaymentMethod { get; set; }

    [Column("Payment_Date", TypeName = "date")]
    public DateTime PaymentDate { get; set; }

    [Required]
    [MaxLength(30)]
    [Column("Payment_Status")]
    public string PaymentStatus { get; set; } = "Unpaid";

    [MaxLength(255)]
    [Column("Notes")]
    public string? Notes { get; set; }
}