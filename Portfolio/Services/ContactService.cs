using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Portfolio.Data;
using Portfolio.Models;

namespace Portfolio.Services;

public sealed class ContactService(
    AppDbContext dbContext,
    IFormSubmissionRateLimitService rateLimitService) : IContactService
{
    public async Task<ContactSubmissionResult> SubmitAsync(
        ContactFormModel model,
        CancellationToken cancellationToken = default)
    {
        var rateLimit = await rateLimitService.TryAcquireAsync(cancellationToken);
        if (!rateLimit.IsAllowed)
        {
            return new ContactSubmissionResult(false, "Too many submissions. Please wait a minute and try again.");
        }

        if (!TryValidateForm(model, out _))
        {
            return new ContactSubmissionResult(false, "Invalid contact form data.");
        }

        var entity = new ContactMessage
        {
            Name = model.Name.Trim(),
            Email = model.Email.Trim(),
            Message = model.Message.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        if (!TryValidateEntity(entity, out _))
        {
            return new ContactSubmissionResult(false, "Invalid contact form data.");
        }

        dbContext.ContactMessages.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new ContactSubmissionResult(true, "Message sent successfully!");
    }

    public async Task<IReadOnlyList<ContactMessage>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.ContactMessages
            .AsNoTracking()
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var message = await dbContext.ContactMessages.FindAsync([id], cancellationToken);
        if (message is null)
        {
            return;
        }

        dbContext.ContactMessages.Remove(message);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static bool TryValidateForm(ContactFormModel model, out List<ValidationResult> results)
    {
        var context = new ValidationContext(model);
        results = [];
        return Validator.TryValidateObject(model, context, results, validateAllProperties: true);
    }

    private static bool TryValidateEntity(ContactMessage entity, out List<ValidationResult> results)
    {
        var context = new ValidationContext(entity);
        results = [];
        return Validator.TryValidateObject(entity, context, results, validateAllProperties: true);
    }
}
