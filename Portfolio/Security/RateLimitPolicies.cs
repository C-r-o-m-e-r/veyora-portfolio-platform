namespace Portfolio.Security;

public static class RateLimitPolicies
{
    public const string FormSubmission = "FormSubmission";
    public const int FormSubmissionPermitLimit = 3;
    public static readonly TimeSpan FormSubmissionWindow = TimeSpan.FromMinutes(1);
}
