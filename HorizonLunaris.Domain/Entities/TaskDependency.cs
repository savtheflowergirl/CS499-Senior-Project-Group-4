namespace HorizonLunaris.Domain.Entities;

/// <summary>
/// Join entity: DependentTask cannot become Available until
/// PrerequisiteTask is Completed. Composite key (DependentTaskId, PrerequisiteTaskId).
/// </summary>
public class TaskDependency
{
    public int DependentTaskId { get; set; }
    public TaskItem DependentTask { get; set; } = null!;

    public int PrerequisiteTaskId { get; set; }
    public TaskItem PrerequisiteTask { get; set; } = null!;
}
