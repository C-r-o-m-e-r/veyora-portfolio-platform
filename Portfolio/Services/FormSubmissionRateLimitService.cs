using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Portfolio.Security;

namespace Portfolio.Services;

public interface IFormSubmissionRateLimitService
{
    ValueTask<FormSubmissionRateLimitResult> TryAcquireAsync(CancellationToken cancellationToken = default);
}

public readonly record struct FormSubmissionRateLimitResult(bool IsAllowed, TimeSpan? RetryAfter);

public sealed class FormSubmissionRateLimitService(IHttpContextAccessor httpContextAccessor)
    : IFormSubmissionRateLimitService, IAsyncDisposable, IDisposable
{
    private readonly PartitionedRateLimiter<HttpContext> _limiter =
        PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = RateLimitPolicies.FormSubmissionPermitLimit,
                    Window = RateLimitPolicies.FormSubmissionWindow,
                    QueueLimit = 0
                }));

    public async ValueTask<FormSubmissionRateLimitResult> TryAcquireAsync(
        CancellationToken cancellationToken = default)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            return new FormSubmissionRateLimitResult(false, RateLimitPolicies.FormSubmissionWindow);
        }

        using var lease = await _limiter.AcquireAsync(httpContext, 1, cancellationToken);
        if (lease.IsAcquired)
        {
            return new FormSubmissionRateLimitResult(true, null);
        }

        TimeSpan? retryAfter = lease.TryGetMetadata(MetadataName.RetryAfter, out var retry)
            ? retry
            : RateLimitPolicies.FormSubmissionWindow;
        return new FormSubmissionRateLimitResult(false, retryAfter);
    }

    public ValueTask DisposeAsync() => _limiter.DisposeAsync();

    public void Dispose() => _limiter.Dispose();
}
