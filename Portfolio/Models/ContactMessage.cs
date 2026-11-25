using System.ComponentModel.DataAnnotations;

namespace Portfolio.Models;

public sealed class ContactMessage
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }

    [Required]
    [MaxLength(100)]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [MaxLength(1000)]
    public required string Message { get; set; }

    public DateTime CreatedAt { get; set; }
}
