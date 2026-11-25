namespace Portfolio.Models;

public sealed class TodoItem
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime Deadline { get; set; }
    public bool IsCompleted { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Auto;

    // flag to differentiate standard task from calendar event
    public bool IsTask { get; set; } = true;

    public TaskPriority CalculatedPriority => CalculatePriority(Priority, Deadline);

    public static TaskPriority CalculatePriority(TaskPriority priority, DateTime deadline)
    {
        if (priority != TaskPriority.Auto)
        {
            return priority;
        }

        var days = (deadline.Date - DateTime.Now.Date).Days;
        if (days > 7) return TaskPriority.Low;
        if (days <= 3) return TaskPriority.High;
        return TaskPriority.Medium;
    }
}