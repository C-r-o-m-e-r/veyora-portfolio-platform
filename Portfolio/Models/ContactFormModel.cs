using System.ComponentModel.DataAnnotations;

namespace Portfolio.Models;

public sealed class ContactFormModel
{
    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [MaxLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Message is required.")]
    [MaxLength(1000, ErrorMessage = "Message cannot exceed 1000 characters.")]
    [MinLength(10, ErrorMessage = "Message must be at least 10 characters.")]
    public string Message { get; set; } = string.Empty;
}
