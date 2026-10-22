using HorizonLunaris.Contracts;
using HorizonLunaris.Data;
using HorizonLunaris.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using HorizonLunaris.Domain.Entities;

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
        public async Task<StatusUpdateResult> UpdateStatusAsync(
        int taskId, UpdateStatusRequest request, CancellationToken ct = default)
{
    // find the task in DB;SM
    var task = await db.Tasks
        .Include(t => t.Owner)
        .Include(t => t.Prerequisites)
        .FirstOrDefaultAsync(t => t.Id == taskId, ct);
    if (task == null)
        throw new NotFoundException("Task not found.");
    // make sure valid user is changing status;SM
    var user = await currentUser.GetRequiredAsync(ct);
    if (user.Role != UserRole.ProjectManager && task.OwnerId != user.Id)
        throw new ForbiddenException("You cannot change this task's status.");
    // ensure task status is availiable;SM
    if (!Enum.IsDefined(request.Status))
        throw new BusinessRuleException("Invalid task status.");
    // make sure task isnt blocked
    if (task.Status == TaskItemStatus.Blocked &&
        (request.Status == TaskItemStatus.InProgress ||
         request.Status == TaskItemStatus.Completed))
    {
        throw new BusinessRuleException("A blocked task cannot be started or completed.");
    }
    // check task prereqs;SM
    if (request.Status == TaskItemStatus.InProgress ||
        request.Status == TaskItemStatus.Completed)
    {
        var ready = await dependencies.ArePrerequisitesCompleteAsync(taskId, ct);
        if (!ready)
            throw new BusinessRuleException("This task has incomplete prerequisites.");
    }
    // else, update task and status;SM
    task.Status = request.Status;
    if (request.Status == TaskItemStatus.Completed)
        task.CompletedAtUtc = DateTime.UtcNow;
    else
        task.CompletedAtUtc = null;
    await db.SaveChangesAsync(ct);
    // prepare to open preceding tasks; SM
    IReadOnlyList<int> unlockedTaskIds = [];
    if (request.Status == TaskItemStatus.Completed)
        unlockedTaskIds = await dependencies.ResolveUnlocksAsync(taskId, ct);
    return new StatusUpdateResult(task.ToDto(), unlockedTaskIds);
}
public async Task<CommentDto> AddCommentAsync(
    int taskId, AddCommentRequest request, CancellationToken ct = default)
{
    // check task in DB;SM
    var taskExists = await db.Tasks.AnyAsync(t => t.Id == taskId, ct);
    if (!taskExists)
        throw new NotFoundException("Task not found.");
    // identify commenting user; SM
    var user = await currentUser.GetRequiredAsync(ct);
    // non-empty comment
    if (string.IsNullOrWhiteSpace(request.Text))
        throw new BusinessRuleException("Comment cannot be empty.");
    if (request.Text.Length > 2000)
        throw new BusinessRuleException("Comment cannot exceed 2000 characters.");
    // create new comment; SM
    var comment = new Comment
    {
            TaskItemId = taskId,
            AuthorId = user.Id,
            Text = request.Text.Trim(),
            CreatedAtUtc = DateTime.UtcNow
    };
    // save comment in DB;SM
    db.Comments.Add(comment);
    await db.SaveChangesAsync(ct);
    return new CommentDto(
        comment.Id,
        comment.TaskItemId,
        comment.AuthorId,
        user.Name,
        comment.Text,
        comment.CreatedAtUtc);
}
public async Task<IReadOnlyList<CommentDto>> GetCommentsAsync(
    int taskId, CancellationToken ct = default)
{
    // verify task; SM
    var taskExists = await db.Tasks.AnyAsync(t => t.Id == taskId, ct);
    if (!taskExists)
        throw new NotFoundException("Task not found.");
    // find comments and users, oldest listed first; SM
    var comments = await db.Comments
        .Include(c => c.Author)
        .Where(c => c.TaskItemId == taskId)
        .OrderBy(c => c.CreatedAtUtc)
        .ThenBy(c => c.Id)
        .ToListAsync(ct);
    // convert comments and return to API;SM
    return comments.Select(c => c.ToDto()).ToList();
}

}
