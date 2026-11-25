using Portfolio.Models;

namespace Portfolio.Services;

public sealed class TimelineService : ITimelineService
{
    private static readonly IReadOnlyList<TimelineEntry> Entries =
    [
        new()
        {
            Period = "Present",
            Title = "Computer Science",
            Organization = "WSB Merito University · Warsaw, Poland",
            Description = "IT student focused on software architecture, backend development, and scalable full-stack applications. Actively expanding into the .NET ecosystem.",
            Type = TimelineEntryType.Education
        },
        new()
        {
            Period = "Ongoing",
            Title = "Full-Stack & Backend Projects",
            Organization = "Personal & Academic Work",
            Description = "Built e-commerce platforms, cross-platform mobile backends, Unity games, and self-hosted infrastructure — emphasizing system design before implementation.",
            Type = TimelineEntryType.Experience
        }
    ];

    public IReadOnlyList<TimelineEntry> GetEntries() => Entries;
}
