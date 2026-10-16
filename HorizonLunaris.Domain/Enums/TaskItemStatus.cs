namespace HorizonLunaris.Domain.Enums;

/// <summary>
/// Named TaskItemStatus (not TaskStatus) to avoid clashing with
/// System.Threading.Tasks.TaskStatus.
/// </summary>
public enum TaskItemStatus
{
    /// <summary>One or more prerequisites are not yet Completed.</summary>
    Blocked = 0,

    /// <summary>No incomplete prerequisites; work may begin.</summary>
    Available = 1,

    InProgress = 2,
    Completed = 3
}
