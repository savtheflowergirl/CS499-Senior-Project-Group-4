using HorizonLunaris.Contracts;
using HorizonLunaris.Data;

namespace HorizonLunaris.Api.Services;

// FEATURE 2 OWNER: replace each NotImplementedException with a real implementation.
// Suggested rules: a Blocked task cannot move to InProgress/Completed (BusinessRuleException);
// only the task owner or a PM may change status; Completed sets CompletedAtUtc;
// after saving a Completed task, call dependencies.ResolveUnlocksAsync and return the ids;
// a comment's author is the current user.
public class TaskProgressService(
    HorizonLunarisDbContext db,
    ICurrentUser currentUser,
    IDependencyService dependencies) : ITaskProgressService
{
    public Task<StatusUpdateResult> UpdateStatusAsync(int taskId, UpdateStatusRequest request, CancellationToken ct = default)
        => throw new NotImplementedException("Feature 2: UpdateStatusAsync");

    public Task<CommentDto> AddCommentAsync(int taskId, AddCommentRequest request, CancellationToken ct = default)
        => throw new NotImplementedException("Feature 2: AddCommentAsync");

    public Task<IReadOnlyList<CommentDto>> GetCommentsAsync(int taskId, CancellationToken ct = default)
        => throw new NotImplementedException("Feature 2: GetCommentsAsync");
}
