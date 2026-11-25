using Portfolio.Models;

namespace Portfolio.Services;

public interface IAvailabilityService
{
    Task<int> GenerateSlotsAsync(IEnumerable<DateOnly> dates, IEnumerable<TimeSpan> startTimes, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AvailableSlot>> GetUnbookedSlotsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AvailableSlot>> GetUnbookedSlotsForDateAsync(DateOnly date, CancellationToken cancellationToken = default);
    Task<IReadOnlySet<DateOnly>> GetAvailableDatesForMonthAsync(int year, int month, CancellationToken cancellationToken = default);
    Task DeleteSlotAsync(int slotId, CancellationToken cancellationToken = default);
}

public static class AvailabilityParsing
{
    public static IReadOnlyList<TimeSpan> ParseTimeSlots(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return [];
        }

        var results = new List<TimeSpan>();
        var parts = input.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var part in parts)
        {
            if (TimeSpan.TryParse(part, out var time))
            {
                results.Add(time);
                continue;
            }

            if (DateTime.TryParseExact(part, ["HH:mm", "H:mm"], null, System.Globalization.DateTimeStyles.None, out var parsed))
            {
                results.Add(parsed.TimeOfDay);
            }
        }

        return results;
    }
}
