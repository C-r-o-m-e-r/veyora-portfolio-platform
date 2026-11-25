namespace Portfolio.Models;

public sealed class TimelineEntry
{
    public required string Period { get; init; }
    public required string Title { get; init; }
    public required string Organization { get; init; }
    public required string Description { get; init; }
    public required TimelineEntryType Type { get; init; }
}

public enum TimelineEntryType
{
    Education,
    Experience,
    Certification
}
