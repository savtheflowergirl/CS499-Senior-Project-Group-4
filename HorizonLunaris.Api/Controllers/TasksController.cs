using HorizonLunaris.Api.Services;
using HorizonLunaris.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace HorizonLunaris.Api.Controllers;

/// <summary>FEATURE 1: read, assign, edit, and delete tasks.</summary>
[ApiController]
[Route("api/tasks")]
public class TasksController(IProjectTaskService service) : ControllerBase
{
    // Get a task by ID
    [HttpGet("{taskId:int}")]
    public async Task<IActionResult> Get(
        int taskId, CancellationToken ct) =>
        Ok(await service.GetTaskAsync(taskId, ct));

    // Assign a task owner
    [HttpPut("{taskId:int}/owner")]
    public async Task<IActionResult> AssignOwner(
        int taskId,
        AssignOwnerRequest request,
        CancellationToken ct) =>
        Ok(await service.AssignOwnerAsync(taskId, request, ct));

    // Edit an existing task; SM
    [HttpPut("{taskId:int}")]
    public async Task<IActionResult> UpdateTask(
        int taskId,
        [FromBody] UpdateTaskRequest request,
        CancellationToken ct) =>
        Ok(await service.UpdateTaskAsync(taskId, request, ct));

    // Delete a task using the selected chain deletion option; SM
    [HttpDelete("{taskId:int}")]
    public async Task<IActionResult> DeleteTask(
        int taskId,
        [FromBody] DeleteTaskRequest request,
        CancellationToken ct)
    {
        await service.DeleteTaskAsync(taskId, request, ct);
        return NoContent();
    }

    // Assign task assignees
    [HttpPut("{taskId:int}/assignees")]
    public async Task<IActionResult> AssignAssignees(
        int taskId,
        AssignAssigneesRequest request,
        CancellationToken ct) =>
        Ok(await service.AssignAssigneesAsync(taskId, request, ct));
}