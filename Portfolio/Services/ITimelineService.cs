using Portfolio.Models;

namespace Portfolio.Services;

public interface ITimelineService
{
    IReadOnlyList<TimelineEntry> GetEntries();
}
