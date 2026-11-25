using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Portfolio.Data;
using Portfolio.Models;

namespace Portfolio.Services;

public sealed class BookingService(
    AppDbContext dbContext,
    IFormSubmissionRateLimitService rateLimitService) : IBookingService
{
    public async Task<IReadOnlyList<BookingRequest>> GetByStatusAsync(
        BookingStatus status,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.BookingRequests
            .AsNoTracking()
            .Include(b => b.AvailableSlot)
            .Where(b => b.Status == status)
            .OrderBy(b => b.RequestedDateTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<BookingSubmissionResult> CreateRequestAsync(
        BookingFormModel model,
        CancellationToken cancellationToken = default)
    {
        var rateLimit = await rateLimitService.TryAcquireAsync(cancellationToken);
        if (!rateLimit.IsAllowed)
        {
            return new BookingSubmissionResult(false, "Too many submissions. Please wait a minute and try again.");
        }

        if (!TryValidateForm(model, out _))
        {
            return new BookingSubmissionResult(false, "Invalid booking request data.");
        }

        var slot = await dbContext.AvailableSlots
            .FirstOrDefaultAsync(s => s.Id == model.SelectedAvailableSlotId && !s.IsBooked, cancellationToken);

        if (slot is null)
        {
            return new BookingSubmissionResult(false, "This time slot is no longer available.");
        }

        var slotDate = DateOnly.FromDateTime(slot.Date);
        if (slotDate < DateOnly.FromDateTime(DateTime.Today))
        {
            return new BookingSubmissionResult(false, "Cannot book a past date.");
        }

        slot.IsBooked = true;

        var entity = new BookingRequest
        {
            GuestName = model.GuestName.Trim(),
            GuestEmail = model.GuestEmail.Trim(),
            Reason = model.Reason.Trim(),
            RequestedDateTime = slot.Date.Add(slot.StartTime),
            Status = BookingStatus.Pending,
            AvailableSlotId = slot.Id
        };

        if (!TryValidateEntity(entity, out _))
        {
            slot.IsBooked = false;
            return new BookingSubmissionResult(false, "Invalid booking request data.");
        }

        dbContext.BookingRequests.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new BookingSubmissionResult(true, "Booking request submitted! You'll receive confirmation once approved.");
    }

    public async Task ApproveAsync(int bookingId, CancellationToken cancellationToken = default)
    {
        var booking = await dbContext.BookingRequests
            .Include(b => b.AvailableSlot)
            .FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken);

        if (booking is null)
        {
            return;
        }

        booking.Status = BookingStatus.Approved;
        booking.AvailableSlot.IsBooked = true;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RejectAsync(int bookingId, CancellationToken cancellationToken = default)
    {
        var booking = await dbContext.BookingRequests
            .Include(b => b.AvailableSlot)
            .FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken);

        if (booking is null)
        {
            return;
        }

        booking.Status = BookingStatus.Rejected;
        booking.AvailableSlot.IsBooked = false;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static bool TryValidateForm(BookingFormModel model, out List<ValidationResult> results)
    {
        var context = new ValidationContext(model);
        results = [];
        return Validator.TryValidateObject(model, context, results, validateAllProperties: true);
    }

    private static bool TryValidateEntity(BookingRequest entity, out List<ValidationResult> results)
    {
        var context = new ValidationContext(entity);
        results = [];
        return Validator.TryValidateObject(entity, context, results, validateAllProperties: true);
    }
}
