using Microsoft.EntityFrameworkCore;
using Portfolio.Data;
using Portfolio.Models;

namespace Portfolio.Services;

public interface ITodoService
{
    Task<IReadOnlyList<TodoItem>> GetOverlappingMonthAsync(int year, int month, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TodoItem>> GetByDateAsync(DateOnly date, CancellationToken cancellationToken = default);
    Task<TodoItem> AddAsync(TaskModel task, CancellationToken cancellationToken = default);
    Task UpdateAsync(TodoItem item, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    // new method for bulk parsing
    Task ParseAndSaveScheduleAsync(string rawText, CancellationToken cancellationToken = default);
}

public sealed class TodoService(AppDbContext dbContext) : ITodoService
{
    public async Task<IReadOnlyList<TodoItem>> GetOverlappingMonthAsync(
        int year,
        int month,
        CancellationToken cancellationToken = default)
    {
        var rangeStart = new DateOnly(year, month, 1).ToDateTime(TimeOnly.MinValue);
        var rangeEnd = new DateOnly(year, month, 1).AddMonths(1).AddDays(-1).ToDateTime(TimeOnly.MaxValue);

        return await dbContext.TodoItems
            .AsNoTracking()
            .Where(t => t.StartDate <= rangeEnd && t.Deadline >= rangeStart)
            .OrderBy(t => t.StartDate)
            .ThenBy(t => t.IsCompleted)
            .ThenBy(t => t.Deadline)
            .ThenBy(t => t.Title)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TodoItem>> GetByDateAsync(
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var dayStart = date.ToDateTime(TimeOnly.MinValue);
        var dayEnd = date.ToDateTime(TimeOnly.MaxValue);

        return await dbContext.TodoItems
            .Where(t => t.StartDate <= dayEnd && t.Deadline >= dayStart)
            .OrderBy(t => t.IsCompleted)
            .ThenBy(t => t.StartDate)
            .ThenBy(t => t.Deadline)
            .ThenBy(t => t.Title)
            .ToListAsync(cancellationToken);
    }

    public async Task<TodoItem> AddAsync(
        TaskModel task,
        CancellationToken cancellationToken = default)
    {
        var item = new TodoItem
        {
            Title = task.Title.Trim(),
            Description = task.Description.Trim(),
            StartDate = task.StartDate,
            Deadline = task.Deadline,
            Priority = task.Priority,
            IsCompleted = false,
            // defaults to true for manual tasks
            IsTask = true
        };

        dbContext.TodoItems.Add(item);
        await dbContext.SaveChangesAsync(cancellationToken);
        return item;
    }

    public async Task UpdateAsync(TodoItem item, CancellationToken cancellationToken = default)
    {
        dbContext.TodoItems.Update(item);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var item = await dbContext.TodoItems.FindAsync([id], cancellationToken);
        if (item is null)
        {
            return;
        }

        dbContext.TodoItems.Remove(item);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    // bulk parse and save method
    public async Task ParseAndSaveScheduleAsync(string rawText, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rawText)) return;

        // protect against html newline encoding
        rawText = rawText.Replace("&#10;", "\n");

        var itemsToAdd = new List<TodoItem>();
        var currentDate = DateTime.Today;

        // split by explicit char array
        var lines = rawText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed)) continue;

            // 1. parse date block, e.g. [28.08.2026]
            if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
            {
                var dateStr = trimmed.Trim('[', ']');

                // normalize dots to slashes so en-GB culture always reads it as dd/MM/yyyy
                var normalizedDateStr = dateStr.Replace(".", "/").Replace("-", "/");

                if (DateTime.TryParse(normalizedDateStr, new System.Globalization.CultureInfo("en-GB"), System.Globalization.DateTimeStyles.None, out var parsedDate))
                {
                    currentDate = parsedDate;
                }
                continue;
            }

            // 2. parse standard task
            // format: Завдання | Назва | Опис: ... | Дедлайн: 10:00
            if (trimmed.StartsWith("Завдання", StringComparison.OrdinalIgnoreCase))
            {
                var parts = trimmed.Split('|', StringSplitOptions.TrimEntries);

                var newTask = new TodoItem
                {
                    // grab title from the second element
                    Title = parts.Length > 1 ? parts[1] : "Нове завдання",
                    StartDate = currentDate,
                    Deadline = currentDate.Date.AddDays(1).AddTicks(-1),
                    IsTask = true
                };

                // check remaining parts for description and deadline
                for (int i = 2; i < parts.Length; i++)
                {
                    var part = parts[i];
                    if (part.StartsWith("Опис:", StringComparison.OrdinalIgnoreCase))
                    {
                        newTask.Description = part.Substring(5).Trim();
                    }
                    else if (part.StartsWith("Дедлайн:", StringComparison.OrdinalIgnoreCase))
                    {
                        if (TimeSpan.TryParse(part.Substring(8).Trim(), out var time))
                        {
                            newTask.Deadline = currentDate.Date.Add(time);
                        }
                    }
                }

                itemsToAdd.Add(newTask);
                continue;
            }

            // 3. parse calendar event: 05:00 - 09:00 | title
            if (trimmed.Contains("-") && trimmed.Contains("|"))
            {
                var parts = trimmed.Split('|', StringSplitOptions.TrimEntries);
                var times = parts[0].Split('-', 2, StringSplitOptions.TrimEntries);

                if (times.Length == 2 &&
                    TimeSpan.TryParse(times[0], out var start) &&
                    TimeSpan.TryParse(times[1], out var end))
                {
                    var calendarItem = new TodoItem
                    {
                        Title = parts.Length > 1 ? parts[1] : "Подія",
                        StartDate = currentDate.Date.Add(start),
                        Deadline = currentDate.Date.Add(end),
                        IsTask = false
                    };

                    for (int i = 2; i < parts.Length; i++)
                    {
                        if (parts[i].StartsWith("Опис:", StringComparison.OrdinalIgnoreCase))
                        {
                            calendarItem.Description = parts[i].Substring(5).Trim();
                        }
                    }

                    itemsToAdd.Add(calendarItem);
                }
            }
        }

        // save all items to database
        if (itemsToAdd.Count > 0)
        {
            dbContext.TodoItems.AddRange(itemsToAdd);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}