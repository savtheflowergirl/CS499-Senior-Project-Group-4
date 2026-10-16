using HorizonLunaris.Domain.Entities;

namespace HorizonLunaris.Api.Services;

/// <summary>Writes the notification to the log instead of sending email. Replace later if time allows.</summary>
public class ConsoleNotificationService(ILogger<ConsoleNotificationService> logger) : INotificationService
{
    public Task NotifyTaskAvailableAsync(TaskItem task, CancellationToken ct = default)
    {
        logger.LogInformation(
            "[NOTIFY] Task {TaskId} \"{Title}\" is now Available. Owner: {Owner}",
            task.Id, task.Title, task.Owner?.Name ?? "unassigned");
        return Task.CompletedTask;
    }
}
