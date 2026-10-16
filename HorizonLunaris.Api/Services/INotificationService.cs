using HorizonLunaris.Domain.Entities;

namespace HorizonLunaris.Api.Services;

/// <summary>The "External Email Service" in the design. Stubbed for the prototype.</summary>
public interface INotificationService
{
    Task NotifyTaskAvailableAsync(TaskItem task, CancellationToken ct = default);
}
