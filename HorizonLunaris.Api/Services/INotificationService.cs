using HorizonLunaris.Domain.Entities;
using HorizonLunaris.Domain.Enums;

namespace HorizonLunaris.Api.Services;

public interface INotificationService
{
    Task NotifyTaskAvailableAsync(TaskItem task, CancellationToken ct = default);
    Task NotifyTaskStatusChangedAsync(
        TaskItem task, TaskItemStatus previousStatus, TaskItemStatus newStatus, CancellationToken ct = default);
}
