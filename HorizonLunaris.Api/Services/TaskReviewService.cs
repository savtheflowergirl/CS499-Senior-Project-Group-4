
using HorizonLunaris.Contracts;
using HorizonLunaris.Data;
using HorizonLunaris.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HorizonLunaris.Api.Services;

// Manual Review Gate implementation; SM
public class TaskReviewService(
    HorizonLunarisDbContext db,
    ICurrentUser currentUser,
    IDependencyService dependencies) : ITaskReviewService
{
    // Configure whether a task requires manual approval; SM
    public async Task<TaskDto> SetReviewRequirementAsync(
        int taskId,
        SetReviewRequirementRequest request,
        CancellationToken ct = default)
    {
        // Only Project Managers can configure review gates.
        await currentUser.GetRequiredProjectManagerAsync(ct);

        var task = await db.Tasks
            .Include(t => t.Assignees)
            .Include(t => t.Prerequisites)
            .FirstOrDefaultAsync(t => t.Id == taskId, ct);

        if (task == null)
            throw new NotFoundException("Task not found.");

        // No changes needed if the requirement is already set.
        if (task.RequiresReview == request.RequiresReview)
            return task.ToDto();

        // A review gate must be enabled before task completion.
        // This prevents retroactively invalidating unlocked tasks.
        if (request.RequiresReview &&
            task.Status == TaskItemStatus.Completed)
        {
            throw new BusinessRuleException(
                "Manual review must be enabled before the task is completed.");
        }

        // Enable or disable the review requirement.
        task.RequiresReview = request.RequiresReview;

        // Changing the requirement clears previous approval.
        task.IsApproved = false;
        task.ApprovedAtUtc = null;

        await db.SaveChangesAsync(ct);

        // Disabling review on a completed task may unlock dependents.
        if (!request.RequiresReview &&
            task.Status == TaskItemStatus.Completed)
        {
            await dependencies.ResolveUnlocksAsync(taskId, ct);
        }

        return task.ToDto();
    }

    // Project Manager approves or rejects a completed task; SM
    public async Task<TaskDto> ReviewTaskAsync(
        int taskId,
        ReviewTaskRequest request,
        CancellationToken ct = default)
    {
        // Only Project Managers can approve or reject tasks.
        await currentUser.GetRequiredProjectManagerAsync(ct);

        var task = await db.Tasks
            .Include(t => t.Assignees)
            .Include(t => t.Prerequisites)
            .FirstOrDefaultAsync(t => t.Id == taskId, ct);

        if (task == null)
            throw new NotFoundException("Task not found.");

        if (!task.RequiresReview)
        {
            throw new BusinessRuleException(
                "This task does not require manual review.");
        }

        if (task.Status != TaskItemStatus.Completed)
        {
            throw new BusinessRuleException(
                "Only completed tasks can be reviewed.");
        }

        if (request.Approved)
        {
            // Avoid changing the approval timestamp unnecessarily.
            if (!task.IsApproved)
            {
                task.IsApproved = true;
                task.ApprovedAtUtc = DateTime.UtcNow;

                await db.SaveChangesAsync(ct);
            }

            // Approval may unlock dependent tasks.
            // DependencyService checks all prerequisites.
            await dependencies.ResolveUnlocksAsync(taskId, ct);
        }
        else
        {
            // Do not revoke an approval after dependent tasks
            // have already been unlocked.
            if (task.IsApproved)
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
                        "This task cannot be rejected because dependent tasks " +
                        "have already been unlocked.");
                }
            }

            // Reject the task and return it for additional work.
            task.IsApproved = false;
            task.ApprovedAtUtc = null;
            task.Status = TaskItemStatus.InProgress;
            task.CompletedAtUtc = null;

            await db.SaveChangesAsync(ct);
        }

        return task.ToDto();
    }
}
