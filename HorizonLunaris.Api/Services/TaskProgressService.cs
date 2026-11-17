using HorizonLunaris.Contracts;
using HorizonLunaris.Data;
using HorizonLunaris.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using HorizonLunaris.Domain.Entities;

namespace HorizonLunaris.Api.Services;

// FEATURE 2: Task status updates and comments.
// Updated to support Manual Review Gate; SM
public class TaskProgressService(
    HorizonLunarisDbContext db,
    ICurrentUser currentUser,
    IDependencyService dependencies,
    INotificationService notifications) : ITaskProgressService
{
    public async Task<StatusUpdateResult> UpdateStatusAsync(
        int taskId,
        UpdateStatusRequest request,
        CancellationToken ct = default)
    {
        // Find task in database; SM
        var task = await db.Tasks
            .Include(t => t.Assignees)
            .Include(t => t.Prerequisites)
            .FirstOrDefaultAsync(t => t.Id == taskId, ct);

        if (task == null)
            throw new NotFoundException("Task not found.");

        // Only an assignee, Project Manager, or Admin may change status; SM
        var user = await currentUser.GetRequiredAsync(ct);

        if (user.Role is not (UserRole.ProjectManager or UserRole.Admin) &&
            task.Assignees.All(assignee => assignee.Id != user.Id))
        {
            throw new ForbiddenException(
                "You cannot change this task's status.");
        }

        // Validate requested status; SM
        if (!Enum.IsDefined(request.Status))
        {
            throw new BusinessRuleException("Invalid task status.");
        }

        // Do not reset completion or approval when status is unchanged; SM
        if (task.Status == request.Status)
        {
            return new StatusUpdateResult(task.ToDto(), []);
        }

        // Blocked tasks cannot be started or completed.
        if (task.Status == TaskItemStatus.Blocked &&
            (request.Status == TaskItemStatus.InProgress ||
             request.Status == TaskItemStatus.Completed))
        {
            throw new BusinessRuleException(
                "A blocked task cannot be started or completed.");
        }

        // Manual Review Gate safeguard; SM
        // Do not invalidate a completed prerequisite after its
        // dependent tasks have already been released.
        if (task.Status == TaskItemStatus.Completed)
        {
            var hasUnlockedDependents = await db.TaskDependencies
                .Where(d => d.PrerequisiteTaskId == taskId)
                .Join(
                    db.Tasks,
                    dependency => dependency.DependentTaskId,
                    dependent => dependent.Id,
                    (_, dependent) => dependent)
                .AnyAsync(
                    dependent => dependent.Status != TaskItemStatus.Blocked,
                    ct);

            if (hasUnlockedDependents)
            {
                throw new BusinessRuleException(
                    "This completed task cannot change status because " +
                    "dependent tasks have already been unlocked.");
            }
        }

        // Ensure all prerequisites are satisfied; SM
        if (request.Status == TaskItemStatus.InProgress ||
            request.Status == TaskItemStatus.Completed)
        {
            var ready = await dependencies.ArePrerequisitesCompleteAsync(
                taskId, ct);

            if (!ready)
            {
                throw new BusinessRuleException(
                    "This task has incomplete or unapproved prerequisites.");
            }
        }

        // Update task status; SM
        var previousStatus = task.Status;
        task.Status = request.Status;

        if (request.Status == TaskItemStatus.Completed)
        {
            task.CompletedAtUtc = DateTime.UtcNow;

            // New completion requires a fresh approval when enabled; SM
            task.IsApproved = false;
            task.ApprovedAtUtc = null;
        }
        else
        {
            task.CompletedAtUtc = null;

            // Unfinished tasks cannot retain approval; SM
            task.IsApproved = false;
            task.ApprovedAtUtc = null;
        }

        await db.SaveChangesAsync(ct);

        // Attempt to unlock eligible dependents; SM
        // DependencyService will wait for approval if required.
        IReadOnlyList<int> unlockedTaskIds = [];

        if (request.Status == TaskItemStatus.Completed)
        {
            unlockedTaskIds = await dependencies.ResolveUnlocksAsync(
                taskId, ct);
        }

        // Notify about the status change (from main); SM
        await notifications.NotifyTaskStatusChangedAsync(
            task, previousStatus, request.Status, ct);

        return new StatusUpdateResult(
            task.ToDto(),
            unlockedTaskIds);
    }

    public async Task<CommentDto> AddCommentAsync(
        int taskId,
        AddCommentRequest request,
        CancellationToken ct = default)
    {
        // Check task exists; SM
        var taskExists = await db.Tasks
            .AnyAsync(t => t.Id == taskId, ct);

        if (!taskExists)
        {
            throw new NotFoundException("Task not found.");
        }

        // Identify commenting user; SM
        var user = await currentUser.GetRequiredAsync(ct);

        // Validate comment; SM
        if (string.IsNullOrWhiteSpace(request.Text))
        {
            throw new BusinessRuleException(
                "Comment cannot be empty.");
        }

        if (request.Text.Length > 2000)
        {
            throw new BusinessRuleException(
                "Comment cannot exceed 2000 characters.");
        }

        // Create comment; SM
        var comment = new Comment
        {
            TaskItemId = taskId,
            AuthorId = user.Id,
            Text = request.Text.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

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
        int taskId,
        CancellationToken ct = default)
    {
        // Verify task exists; SM
        var taskExists = await db.Tasks
            .AnyAsync(t => t.Id == taskId, ct);

        if (!taskExists)
        {
            throw new NotFoundException("Task not found.");
        }

        // Return comments oldest first; SM
        var comments = await db.Comments
            .Include(c => c.Author)
            .Where(c => c.TaskItemId == taskId)
            .OrderBy(c => c.CreatedAtUtc)
            .ThenBy(c => c.Id)
            .ToListAsync(ct);

        return comments
            .Select(c => c.ToDto())
            .ToList();
    }
}