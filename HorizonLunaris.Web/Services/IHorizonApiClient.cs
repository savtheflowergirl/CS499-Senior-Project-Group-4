using HorizonLunaris.Contracts;

namespace HorizonLunaris.Web.Services;

/// <summary>
/// The front end's single door to the API. UI pages call these methods and never build URLs themselves.
/// Errors surface as ApiException (API returned an error) or HttpRequestException (API unreachable).
/// </summary>
public interface IHorizonApiClient
{
    // Users
    Task<IReadOnlyList<UserDto>> GetUsersAsync(CancellationToken ct = default);
    Task<UserDto> CreateUserAsync(CreateUserRequest request, CancellationToken ct = default);
    Task<UserDto> UpdateUserEmailAsync(int userId, UpdateUserEmailRequest request, CancellationToken ct = default);

    // Feature 1
    Task<IReadOnlyList<ProjectDto>> GetProjectsAsync(CancellationToken ct = default);
    Task<ProjectDto> GetProjectAsync(int projectId, CancellationToken ct = default);
    Task<ProjectDto> CreateProjectAsync(CreateProjectRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<TaskDto>> GetTasksAsync(int projectId, CancellationToken ct = default);
    Task<TaskDto> CreateTaskAsync(int projectId, CreateTaskRequest request, CancellationToken ct = default);
    Task<TaskDto> GetTaskAsync(int taskId, CancellationToken ct = default);
    Task<TaskDto> AssignOwnerAsync(int taskId, AssignOwnerRequest request, CancellationToken ct = default);
    // Task editing and deletion; SM
    Task<TaskDto> UpdateTaskAsync(
        int taskId,
        UpdateTaskRequest request,
        CancellationToken ct = default);

    Task DeleteTaskAsync(
        int taskId,
        DeleteTaskRequest request,
        CancellationToken ct = default);
    Task<TaskDto> AssignAssigneesAsync(int taskId, AssignAssigneesRequest request, CancellationToken ct = default);

    // Feature 2
    Task<StatusUpdateResult> UpdateStatusAsync(int taskId, UpdateStatusRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<CommentDto>> GetCommentsAsync(int taskId, CancellationToken ct = default);
    Task<CommentDto> AddCommentAsync(int taskId, AddCommentRequest request, CancellationToken ct = default);

    // Feature 3
    Task<IReadOnlyList<TaskDto>> GetPrerequisitesAsync(int taskId, CancellationToken ct = default);
    Task<TaskDto> AddDependencyAsync(int taskId, AddDependencyRequest request, CancellationToken ct = default);
    Task RemoveDependencyAsync(int taskId, int prerequisiteTaskId, CancellationToken ct = default);
    Task<EmailSenderSettingsDto> GetEmailSenderSettingsAsync(CancellationToken ct = default);
    Task<EmailSenderSettingsDto> UpdateEmailSenderSettingsAsync(UpdateEmailSenderRequest request, CancellationToken ct = default);
}
