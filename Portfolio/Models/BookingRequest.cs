using System.ComponentModel.DataAnnotations;

namespace Portfolio.Models;

public sealed class BookingRequest
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public required string GuestName { get; set; }

    [Required]
    [MaxLength(100)]
    [EmailAddress]
    public required string GuestEmail { get; set; }

    [Required]
    [MaxLength(1000)]
    public required string Reason { get; set; }

    public DateTime RequestedDateTime { get; set; }
    public BookingStatus Status { get; set; }
    public int AvailableSlotId { get; set; }
    public AvailableSlot AvailableSlot { get; set; } = null!;
}
