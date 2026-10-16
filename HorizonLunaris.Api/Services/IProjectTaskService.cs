using HorizonLunaris.Contracts;

namespace HorizonLunaris.Api.Services;

/// <summary>FEATURE 1: create projects and tasks/sub-tasks, assign owners.</summary>
public interface IProjectTaskService
{
    Task<ProjectDto> CreateProjectAsync(CreateProjectRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<ProjectDto>> GetProjectsAsync(CancellationToken ct = default);
    Task<ProjectDto> GetProjectAsync(int projectId, CancellationToken ct = default);

    Task<TaskDto> CreateTaskAsync(int projectId, CreateTaskRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<TaskDto>> GetTasksAsync(int projectId, CancellationToken ct = default);
    Task<TaskDto> GetTaskAsync(int taskId, CancellationToken ct = default);
    Task<TaskDto> AssignOwnerAsync(int taskId, AssignOwnerRequest request, CancellationToken ct = default);
}
