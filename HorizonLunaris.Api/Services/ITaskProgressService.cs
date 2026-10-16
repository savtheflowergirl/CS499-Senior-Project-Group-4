using HorizonLunaris.Contracts;

namespace HorizonLunaris.Api.Services;

/// <summary>FEATURE 2: update completion status and add progress comments.</summary>
public interface ITaskProgressService
{
    /// <summary>
    /// Changes a task's status. When the new status is Completed, this must call
    /// IDependencyService.ResolveUnlocksAsync and return the unlocked task ids.
    /// </summary>
    Task<StatusUpdateResult> UpdateStatusAsync(int taskId, UpdateStatusRequest request, CancellationToken ct = default);

    Task<CommentDto> AddCommentAsync(int taskId, AddCommentRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<CommentDto>> GetCommentsAsync(int taskId, CancellationToken ct = default);
}
