using HorizonLunaris.Api.Services;
using HorizonLunaris.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace HorizonLunaris.Api.Controllers;

/// <summary>FEATURE 1: read a task and assign its owner.</summary>
[ApiController]
[Route("api/tasks")]
public class TasksController(IProjectTaskService service) : ControllerBase
{
    [HttpGet("{taskId:int}")]
    public async Task<IActionResult> Get(int taskId, CancellationToken ct) =>
        Ok(await service.GetTaskAsync(taskId, ct));

    [HttpPut("{taskId:int}/owner")]
    public async Task<IActionResult> AssignOwner(int taskId, AssignOwnerRequest request, CancellationToken ct) =>
        Ok(await service.AssignOwnerAsync(taskId, request, ct));
}
