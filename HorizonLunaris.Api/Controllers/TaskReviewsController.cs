
using HorizonLunaris.Api.Services;
using HorizonLunaris.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace HorizonLunaris.Api.Controllers;

// Manual Review Gate API endpoints; SM
[ApiController]
[Route("api/tasks")]
public class TaskReviewsController(ITaskReviewService service) : ControllerBase
{
    // Enable or disable manual review for a task.
    // Project Manager only.
    [HttpPut("{taskId:int}/review-requirement")]
    public async Task<IActionResult> SetReviewRequirement(
        int taskId,
        [FromBody] SetReviewRequirementRequest request,
        CancellationToken ct)
    {
        var task = await service.SetReviewRequirementAsync(
            taskId, request, ct);

        return Ok(task);
    }

    // Approve or reject a completed task.
    // Project Manager only.
    [HttpPost("{taskId:int}/review")]
    public async Task<IActionResult> ReviewTask(
        int taskId,
        [FromBody] ReviewTaskRequest request,
        CancellationToken ct)
    {
        var task = await service.ReviewTaskAsync(
            taskId, request, ct);

        return Ok(task);
    }
}
