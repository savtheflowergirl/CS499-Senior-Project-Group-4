using HorizonLunaris.Api.Services;
using HorizonLunaris.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace HorizonLunaris.Api.Controllers;

/// <summary>FEATURE 3: dependencies between tasks.</summary>
[ApiController]
[Route("api/tasks/{taskId:int}/dependencies")]
public class TaskDependenciesController(IDependencyService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPrerequisites(int taskId, CancellationToken ct) =>
        Ok(await service.GetPrerequisitesAsync(taskId, ct));

    [HttpPost]
    public async Task<IActionResult> Add(int taskId, AddDependencyRequest request, CancellationToken ct) =>
        Ok(await service.AddDependencyAsync(taskId, request, ct));

    [HttpDelete("{prerequisiteTaskId:int}")]
    public async Task<IActionResult> Remove(int taskId, int prerequisiteTaskId, CancellationToken ct)
    {
        await service.RemoveDependencyAsync(taskId, prerequisiteTaskId, ct);
        return NoContent();
    }
}
