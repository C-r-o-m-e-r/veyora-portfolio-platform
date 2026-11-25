using System.ComponentModel.DataAnnotations;

namespace Portfolio.Models;

public sealed class TaskModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime Deadline { get; set; }

    public TaskPriority Priority { get; set; } = TaskPriority.Auto;
}
