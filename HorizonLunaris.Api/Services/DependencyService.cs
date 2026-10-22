using HorizonLunaris.Contracts;
using HorizonLunaris.Data;
using HorizonLunaris.Domain.Entities;
using HorizonLunaris.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HorizonLunaris.Api.Services;

// FEATURE 3: dependencies between tasks.
public class DependencyService(
    HorizonLunarisDbContext db,
    ICurrentUser currentUser,
    INotificationService notifications) : IDependencyService
{
    public async Task<IReadOnlyList<TaskDto>> GetPrerequisitesAsync(
        int taskId,
        CancellationToken ct = default)
    {
        var task = await db.Tasks
            .Include(t => t.Prerequisites)
                .ThenInclude(d => d.PrerequisiteTask)
                    .ThenInclude(t => t.Owner)
            .SingleOrDefaultAsync(t => t.Id == taskId, ct);

        if (task is null)
        {
            throw new NotFoundException($"Task {taskId} was not found.");
        }

        return task.Prerequisites
            .Select(d => d.PrerequisiteTask.ToDto())
            .ToList();
    }

    public async Task<TaskDto> AddDependencyAsync(
        int taskId,
        AddDependencyRequest request,
        CancellationToken ct = default)
    {
        await currentUser.GetRequiredProjectManagerAsync(ct);

        var dependentTask = await db.Tasks
            .Include(t => t.Owner)
            .Include(t => t.Prerequisites)
            .SingleOrDefaultAsync(t => t.Id == taskId, ct);

        if (dependentTask is null)
        {
            throw new NotFoundException($"Dependent task {taskId} was not found.");
        }

        var prerequisiteTask = await db.Tasks
            .SingleOrDefaultAsync(t => t.Id == request.PrerequisiteTaskId, ct);

        if (prerequisiteTask is null)
        {
            throw new NotFoundException(
                $"Prerequisite task {request.PrerequisiteTaskId} was not found.");
        }

        ConfigureDependency(dependentTask, prerequisiteTask);

        await db.SaveChangesAsync(ct);

        return dependentTask.ToDto();
    }

    public async Task RemoveDependencyAsync(
        int taskId,
        int prerequisiteTaskId,
        CancellationToken ct = default)
    {
        await currentUser.GetRequiredProjectManagerAsync(ct);

        var dependency = await db.TaskDependencies
            .Include(d => d.DependentTask)
                .ThenInclude(t => t.Prerequisites)
            .SingleOrDefaultAsync(
                d => d.DependentTaskId == taskId &&
                     d.PrerequisiteTaskId == prerequisiteTaskId,
                ct);

        if (dependency is null)
        {
            throw new NotFoundException(
                $"Dependency from task {taskId} to task {prerequisiteTaskId} was not found.");
        }

        var dependentTask = dependency.DependentTask;

        db.TaskDependencies.Remove(dependency);

        var remainingPrerequisites = dependentTask.Prerequisites
            .Where(d => d != dependency)
            .Select(d => d.PrerequisiteTaskId)
            .ToList();

        if (dependentTask.Status == TaskItemStatus.Blocked)
        {
            var hasIncompletePrerequisite = remainingPrerequisites.Count > 0 &&
                await db.TaskDependencies
                    .Where(d =>
                        d.DependentTaskId == dependentTask.Id &&
                        d.PrerequisiteTaskId != prerequisiteTaskId)
                    .Join(
                        db.Tasks,
                        dependencyRow => dependencyRow.PrerequisiteTaskId,
                        prerequisite => prerequisite.Id,
                        (_, prerequisite) => prerequisite)
                    .AnyAsync(t => t.Status != TaskItemStatus.Completed, ct);

            if (!hasIncompletePrerequisite)
            {
                dependentTask.Status = TaskItemStatus.Available;

                await db.SaveChangesAsync(ct);
                await notifications.NotifyTaskAvailableAsync(dependentTask, ct);
                return;
            }
        }

        await db.SaveChangesAsync(ct);
    }

    public async Task<bool> ArePrerequisitesCompleteAsync(
        int taskId,
        CancellationToken ct = default)
    {
        var taskExists = await db.Tasks
            .AnyAsync(t => t.Id == taskId, ct);

        if (!taskExists)
        {
            throw new NotFoundException($"Task {taskId} was not found.");
        }

        return !await db.TaskDependencies
            .Where(d => d.DependentTaskId == taskId)
            .Join(
                db.Tasks,
                dependency => dependency.PrerequisiteTaskId,
                prerequisite => prerequisite.Id,
                (_, prerequisite) => prerequisite)
            .AnyAsync(t => t.Status != TaskItemStatus.Completed, ct);
    }

    public async Task<IReadOnlyList<int>> ResolveUnlocksAsync(
        int completedTaskId,
        CancellationToken ct = default)
    {
        var completedTaskExists = await db.Tasks
            .AnyAsync(t => t.Id == completedTaskId, ct);

        if (!completedTaskExists)
        {
            throw new NotFoundException(
                $"Completed task {completedTaskId} was not found.");
        }

        var dependentTasks = await db.Tasks
            .Where(t => t.Prerequisites.Any(d => d.PrerequisiteTaskId == completedTaskId))
            .Include(t => t.Owner)
            .Include(t => t.Prerequisites)
                .ThenInclude(d => d.PrerequisiteTask)
            .ToListAsync(ct);

        var unlockedTasks = new List<TaskItem>();

        foreach (var dependentTask in dependentTasks
                     .Where(t => t.Status == TaskItemStatus.Blocked))
        {
            var allPrerequisitesCompleted = dependentTask.Prerequisites
                .All(d => d.PrerequisiteTask.Status == TaskItemStatus.Completed);

            if (!allPrerequisitesCompleted)
            {
                continue;
            }

            dependentTask.Status = TaskItemStatus.Available;
            unlockedTasks.Add(dependentTask);
        }

        if (unlockedTasks.Count == 0)
        {
            return [];
        }

        await db.SaveChangesAsync(ct);

        foreach (var unlockedTask in unlockedTasks)
        {
            await notifications.NotifyTaskAvailableAsync(unlockedTask, ct);
        }

        return unlockedTasks
            .Select(t => t.Id)
            .ToList();
    }

    private void ConfigureDependency(
        TaskItem dependentTask,
        TaskItem prerequisiteTask)
    {
        if (dependentTask.Id == prerequisiteTask.Id)
        {
            throw new BusinessRuleException(
                "A task cannot depend on itself.");
        }

        if (dependentTask.ProjectId != prerequisiteTask.ProjectId)
        {
            throw new BusinessRuleException(
                "Both tasks must belong to the same project.");
        }

        var dependencyAlreadyExists = dependentTask.Prerequisites
            .Any(d => d.PrerequisiteTaskId == prerequisiteTask.Id);

        if (dependencyAlreadyExists)
        {
            throw new BusinessRuleException(
                "This task dependency already exists.");
        }

        if (WouldCreateCycle(dependentTask.Id, prerequisiteTask.Id))
        {
            throw new BusinessRuleException(
                "The dependency would create a circular dependency.");
        }

        dependentTask.Prerequisites.Add(new TaskDependency
        {
            DependentTask = dependentTask,
            PrerequisiteTask = prerequisiteTask
        });


        if (dependentTask.Status == TaskItemStatus.Available &&
            prerequisiteTask.Status != TaskItemStatus.Completed)
        {
            dependentTask.Status = TaskItemStatus.Blocked;
        }
    }

    private bool WouldCreateCycle(
        int dependentTaskId,
        int prerequisiteTaskId)
    {
        var dependencies = db.TaskDependencies
            .AsEnumerable()
            .ToList();

        var prerequisitesByDependent = dependencies
            .GroupBy(d => d.DependentTaskId)
            .ToDictionary(
                group => group.Key,
                group => group.Select(d => d.PrerequisiteTaskId).ToList());

        var tasksToVisit = new Stack<int>();
        var visited = new HashSet<int>();

        tasksToVisit.Push(prerequisiteTaskId);

        while (tasksToVisit.Count > 0)
        {
            var currentTaskId = tasksToVisit.Pop();

            if (!visited.Add(currentTaskId))
            {
                continue;
            }

            if (currentTaskId == dependentTaskId)
            {
                return true;
            }

            if (!prerequisitesByDependent.TryGetValue(
                    currentTaskId,
                    out var prerequisites))
            {
                continue;
            }

            foreach (var prerequisiteId in prerequisites)
            {
                tasksToVisit.Push(prerequisiteId);
            }
        }

        return false;
    }
}
