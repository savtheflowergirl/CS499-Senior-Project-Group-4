using HorizonLunaris.Domain.Entities;
using HorizonLunaris.Domain.Enums;

namespace HorizonLunaris.Api.Services;

/// <summary>Writes the notification to the log instead of sending email. Replace later if time allows.</summary>
public class ConsoleNotificationService(ILogger<ConsoleNotificationService> logger) : INotificationService
{
    public Task NotifyTaskAvailableAsync(TaskItem task, CancellationToken ct = default)
    {
        var assignees = task.Assignees.Count == 0
            ? "unassigned"
            : string.Join(", ", task.Assignees.Select(user => user.Name));
        logger.LogInformation(
            "[NOTIFY] Task {TaskId} \"{Title}\" is now Available. Assignees: {Assignees}",
            task.Id, task.Title, assignees);
        return Task.CompletedTask;
    }

    public Task NotifyTaskStatusChangedAsync(
        TaskItem task, TaskItemStatus previousStatus, TaskItemStatus newStatus, CancellationToken ct = default)
    {
        logger.LogInformation(
            "[NOTIFY] Task {TaskId} \"{Title}\" status changed from {PreviousStatus} to {NewStatus}.",
            task.Id, task.Title, previousStatus, newStatus);
        return Task.CompletedTask;
    }
}
