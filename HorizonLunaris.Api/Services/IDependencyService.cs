using HorizonLunaris.Contracts;

namespace HorizonLunaris.Api.Services;

/// <summary>FEATURE 3: task dependencies and automatic unlocking (the "Update Checker").</summary>
public interface IDependencyService
{
    Task<IReadOnlyList<TaskDto>> GetPrerequisitesAsync(int taskId, CancellationToken ct = default);

    /// <summary>PM only. Rejects self-dependency and circular chains. Sets the dependent Blocked if the prerequisite is incomplete.</summary>
    Task<TaskDto> AddDependencyAsync(int taskId, AddDependencyRequest request, CancellationToken ct = default);

    /// <summary>PM only. May unlock the dependent task if nothing else blocks it.</summary>
    Task RemoveDependencyAsync(int taskId, int prerequisiteTaskId, CancellationToken ct = default);

    /// <summary>True when every prerequisite of the task is Completed (or it has none). Feature 2 uses this to guard status changes.</summary>
    Task<bool> ArePrerequisitesCompleteAsync(int taskId, CancellationToken ct = default);

    /// <summary>
    /// Called after a task is Completed. For each Blocked dependent whose prerequisites are now all Completed:
    /// set it Available, save, and send a notification. Returns the unlocked task ids.
    /// </summary>
    Task<IReadOnlyList<int>> ResolveUnlocksAsync(int completedTaskId, CancellationToken ct = default);
}
