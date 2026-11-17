
using HorizonLunaris.Contracts;

namespace HorizonLunaris.Api.Services;

// Manual Review Gate interface; SM
public interface ITaskReviewService
{
    // Enable or disable manual review for a task.
    Task<TaskDto> SetReviewRequirementAsync(
        int taskId,
        SetReviewRequirementRequest request,
        CancellationToken ct = default);

    // Approve or reject a completed task.
    Task<TaskDto> ReviewTaskAsync(
        int taskId,
        ReviewTaskRequest request,
        CancellationToken ct = default);
}
