using System.ComponentModel.DataAnnotations;

namespace Portfolio.Models;

public sealed class ChangePasswordFormModel
{
    [Required(ErrorMessage = "Current password is required.")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required.")]
    [MinLength(8, ErrorMessage = "New password must be at least 8 characters.")]
    [MaxLength(128)]
    public string NewPassword { get; set; } = string.Empty;
}
