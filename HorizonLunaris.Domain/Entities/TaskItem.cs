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

    // ----- Manual Review Gate; SM -----

    // Project Manager chooses whether this task needs approval.
    public bool RequiresReview { get; set; } = false;

    // True only after a Project Manager approves the completed task.
    public bool IsApproved { get; set; } = false;

    // Records when the task received approval.
    public DateTime? ApprovedAtUtc { get; set; }

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    // Sub-task hierarchy (self-reference)
    public int? ParentTaskId { get; set; }
    public TaskItem? ParentTask { get; set; }
    public ICollection<TaskItem> SubTasks { get; set; } = new List<TaskItem>();

    public ICollection<User> Assignees { get; set; } = new List<User>();

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


