using HorizonLunaris.Contracts;

namespace HorizonLunaris.Web.Services;

/// <summary>
/// The front end's single door to the API. UI pages call these methods and never build URLs themselves.
/// Errors surface as ApiException (API returned an error) or HttpRequestException (API unreachable).
/// </summary>
public interface IHorizonApiClient
{
    // Users (works now)
    Task<IReadOnlyList<UserDto>> GetUsersAsync(CancellationToken ct = default);

    // Feature 1
    Task<IReadOnlyList<ProjectDto>> GetProjectsAsync(CancellationToken ct = default);
    Task<ProjectDto> GetProjectAsync(int projectId, CancellationToken ct = default);
    Task<ProjectDto> CreateProjectAsync(CreateProjectRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<TaskDto>> GetTasksAsync(int projectId, CancellationToken ct = default);
    Task<TaskDto> CreateTaskAsync(int projectId, CreateTaskRequest request, CancellationToken ct = default);
    Task<TaskDto> GetTaskAsync(int taskId, CancellationToken ct = default);
    Task<TaskDto> AssignOwnerAsync(int taskId, AssignOwnerRequest request, CancellationToken ct = default);

    // Feature 2
    Task<StatusUpdateResult> UpdateStatusAsync(int taskId, UpdateStatusRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<CommentDto>> GetCommentsAsync(int taskId, CancellationToken ct = default);
    Task<CommentDto> AddCommentAsync(int taskId, AddCommentRequest request, CancellationToken ct = default);

    // Feature 3
    Task<IReadOnlyList<TaskDto>> GetPrerequisitesAsync(int taskId, CancellationToken ct = default);
    Task<TaskDto> AddDependencyAsync(int taskId, AddDependencyRequest request, CancellationToken ct = default);
    Task RemoveDependencyAsync(int taskId, int prerequisiteTaskId, CancellationToken ct = default);
}
