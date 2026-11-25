using Microsoft.EntityFrameworkCore;
using Portfolio.Data;
using Portfolio.Models;

namespace Portfolio.Services;

public sealed class AvailabilityService(AppDbContext dbContext) : IAvailabilityService
{
    public async Task<int> GenerateSlotsAsync(
        IEnumerable<DateOnly> dates,
        IEnumerable<TimeSpan> startTimes,
        CancellationToken cancellationToken = default)
    {
        var uniqueDates = dates.Distinct().ToList();
        var uniqueTimes = startTimes.Distinct().ToList();

        if (uniqueDates.Count == 0 || uniqueTimes.Count == 0)
        {
            return 0;
        }

        var created = 0;

        foreach (var date in uniqueDates)
        {
            var dateValue = ToDateTime(date);

            foreach (var startTime in uniqueTimes)
            {
                var exists = await dbContext.AvailableSlots.AnyAsync(
                    s => s.Date == dateValue && s.StartTime == startTime,
                    cancellationToken);

                if (exists)
                {
                    continue;
                }

                dbContext.AvailableSlots.Add(new AvailableSlot
                {
                    Date = dateValue,
                    StartTime = startTime,
                    IsBooked = false
                });

                created++;
            }
        }

        if (created > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return created;
    }

    public async Task<IReadOnlyList<AvailableSlot>> GetUnbookedSlotsAsync(
        CancellationToken cancellationToken = default)
    {
        var slots = await dbContext.AvailableSlots
            .AsNoTracking()
            .Where(s => !s.IsBooked)
            .ToListAsync(cancellationToken);

        return slots
            .OrderBy(s => s.Date)
            .ThenBy(s => s.StartTime)
            .ToList();
    }

    public async Task<IReadOnlyList<AvailableSlot>> GetUnbookedSlotsForDateAsync(
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var dateValue = ToDateTime(date);

        var slots = await dbContext.AvailableSlots
            .AsNoTracking()
            .Where(s => s.Date == dateValue && !s.IsBooked)
            .ToListAsync(cancellationToken);

        return slots
            .OrderBy(s => s.StartTime)
            .ToList();
    }

    public async Task<IReadOnlySet<DateOnly>> GetAvailableDatesForMonthAsync(
        int year,
        int month,
        CancellationToken cancellationToken = default)
    {
        var start = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Local);
        var end = start.AddMonths(1);
        var today = DateOnly.FromDateTime(DateTime.Today);

        var slots = await dbContext.AvailableSlots
            .AsNoTracking()
            .Where(s => !s.IsBooked && s.Date >= start && s.Date < end)
            .Select(s => s.Date)
            .Distinct()
            .ToListAsync(cancellationToken);

        return slots
            .Select(d => DateOnly.FromDateTime(d))
            .Where(d => d >= today)
            .ToHashSet();
    }

    public async Task DeleteSlotAsync(int slotId, CancellationToken cancellationToken = default)
    {
        var slot = await dbContext.AvailableSlots.FindAsync([slotId], cancellationToken);
        if (slot is null || slot.IsBooked)
        {
            return;
        }

        dbContext.AvailableSlots.Remove(slot);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    internal static DateTime ToDateTime(DateOnly date) =>
        date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Local);
}