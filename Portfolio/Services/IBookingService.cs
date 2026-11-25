using Portfolio.Models;

namespace Portfolio.Services;

public interface IBookingService
{
    Task<IReadOnlyList<BookingRequest>> GetByStatusAsync(BookingStatus status, CancellationToken cancellationToken = default);
    Task<BookingSubmissionResult> CreateRequestAsync(BookingFormModel model, CancellationToken cancellationToken = default);
    Task ApproveAsync(int bookingId, CancellationToken cancellationToken = default);
    Task RejectAsync(int bookingId, CancellationToken cancellationToken = default);
}

public sealed record BookingSubmissionResult(bool Success, string Message);

public sealed class BookingFormModel
{
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Name is required.")]
    [System.ComponentModel.DataAnnotations.MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public string GuestName { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Email is required.")]
    [System.ComponentModel.DataAnnotations.EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [System.ComponentModel.DataAnnotations.MaxLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
    public string GuestEmail { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Reason is required.")]
    [System.ComponentModel.DataAnnotations.MaxLength(1000, ErrorMessage = "Reason cannot exceed 1000 characters.")]
    [System.ComponentModel.DataAnnotations.MinLength(5, ErrorMessage = "Reason must be at least 5 characters.")]
    public string Reason { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Range(1, int.MaxValue, ErrorMessage = "Select a time slot.")]
    public int SelectedAvailableSlotId { get; set; }
}
