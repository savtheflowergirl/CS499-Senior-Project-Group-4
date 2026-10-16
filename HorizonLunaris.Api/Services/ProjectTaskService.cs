using HorizonLunaris.Contracts;
using HorizonLunaris.Data;

namespace HorizonLunaris.Api.Services;

// FEATURE 1 OWNER: replace each NotImplementedException with a real implementation.
// Suggested rules: only a Project Manager creates projects/tasks and assigns owners;
// a parent task must belong to the same project; the owner must exist.
public class ProjectTaskService(HorizonLunarisDbContext db, ICurrentUser currentUser) : IProjectTaskService
{
    public Task<ProjectDto> CreateProjectAsync(CreateProjectRequest request, CancellationToken ct = default)
        => throw new NotImplementedException("Feature 1: CreateProjectAsync");

    public Task<IReadOnlyList<ProjectDto>> GetProjectsAsync(CancellationToken ct = default)
        => throw new NotImplementedException("Feature 1: GetProjectsAsync");

    public Task<ProjectDto> GetProjectAsync(int projectId, CancellationToken ct = default)
        => throw new NotImplementedException("Feature 1: GetProjectAsync");

    public Task<TaskDto> CreateTaskAsync(int projectId, CreateTaskRequest request, CancellationToken ct = default)
        => throw new NotImplementedException("Feature 1: CreateTaskAsync");

    public Task<IReadOnlyList<TaskDto>> GetTasksAsync(int projectId, CancellationToken ct = default)
        => throw new NotImplementedException("Feature 1: GetTasksAsync");

    public Task<TaskDto> GetTaskAsync(int taskId, CancellationToken ct = default)
        => throw new NotImplementedException("Feature 1: GetTaskAsync");

    public Task<TaskDto> AssignOwnerAsync(int taskId, AssignOwnerRequest request, CancellationToken ct = default)
        => throw new NotImplementedException("Feature 1: AssignOwnerAsync");
}
