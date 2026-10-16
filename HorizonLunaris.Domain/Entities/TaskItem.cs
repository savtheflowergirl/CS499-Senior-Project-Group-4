using HorizonLunaris.Domain.Enums;

namespace HorizonLunaris.Domain.Entities;

/// <summary>
/// A task, or a sub-task when ParentTaskId is set.
/// </summary>
public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>
    /// New tasks start Available. The service layer sets Blocked when
    /// an incomplete prerequisite is added.
    /// </summary>
    public TaskItemStatus Status { get; set; } = TaskItemStatus.Available;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? DueDateUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    // Sub-task hierarchy (self-reference)
    public int? ParentTaskId { get; set; }
    public TaskItem? ParentTask { get; set; }
    public ICollection<TaskItem> SubTasks { get; set; } = new List<TaskItem>();

    // Assigned owner (optional until assigned)
    public int? OwnerId { get; set; }
    public User? Owner { get; set; }

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    /// <summary>
    /// Rows describing the tasks THIS task depends on
    /// (this task is the DependentTask in each row).
    /// </summary>
    public ICollection<TaskDependency> Prerequisites { get; set; } = new List<TaskDependency>();

    /// <summary>
    /// Rows describing the tasks that depend on THIS task
    /// (this task is the PrerequisiteTask in each row).
    /// </summary>
    public ICollection<TaskDependency> Dependents { get; set; } = new List<TaskDependency>();
}
