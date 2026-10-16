using HorizonLunaris.Contracts;
using HorizonLunaris.Data;

namespace HorizonLunaris.Api.Services;

// FEATURE 3 OWNER: replace each NotImplementedException with a real implementation.
// Suggested rules: PM only (currentUser.GetRequiredProjectManagerAsync); a task cannot depend on itself;
// both tasks must be in the same project; reject circular chains (walk the prerequisites, A -> B -> A);
// adding an incomplete prerequisite to an Available task sets it Blocked;
// ResolveUnlocksAsync sets Blocked dependents Available only when ALL prerequisites are Completed, then notifies.
public class DependencyService(
    HorizonLunarisDbContext db,
    ICurrentUser currentUser,
    INotificationService notifications) : IDependencyService
{
    public Task<IReadOnlyList<TaskDto>> GetPrerequisitesAsync(int taskId, CancellationToken ct = default)
        => throw new NotImplementedException("Feature 3: GetPrerequisitesAsync");

    public Task<TaskDto> AddDependencyAsync(int taskId, AddDependencyRequest request, CancellationToken ct = default)
        => throw new NotImplementedException("Feature 3: AddDependencyAsync");

    public Task RemoveDependencyAsync(int taskId, int prerequisiteTaskId, CancellationToken ct = default)
        => throw new NotImplementedException("Feature 3: RemoveDependencyAsync");

    public Task<bool> ArePrerequisitesCompleteAsync(int taskId, CancellationToken ct = default)
        => throw new NotImplementedException("Feature 3: ArePrerequisitesCompleteAsync");

    public Task<IReadOnlyList<int>> ResolveUnlocksAsync(int completedTaskId, CancellationToken ct = default)
        => throw new NotImplementedException("Feature 3: ResolveUnlocksAsync");
}
