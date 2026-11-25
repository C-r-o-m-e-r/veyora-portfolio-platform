using Portfolio.Models;

namespace Portfolio.Services;

public interface IContactService
{
    Task<ContactSubmissionResult> SubmitAsync(ContactFormModel model, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ContactMessage>> GetAllAsync(CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}

public sealed record ContactSubmissionResult(bool Success, string Message);
