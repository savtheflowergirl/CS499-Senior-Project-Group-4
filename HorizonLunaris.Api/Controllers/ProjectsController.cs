using HorizonLunaris.Api.Services;
using HorizonLunaris.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace HorizonLunaris.Api.Controllers;

/// <summary>FEATURE 1: projects and the tasks inside them.</summary>
[ApiController]
[Route("api/projects")]
public class ProjectsController(IProjectTaskService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) => Ok(await service.GetProjectsAsync(ct));

    [HttpPost]
    public async Task<IActionResult> Create(CreateProjectRequest request, CancellationToken ct)
    {
        var project = await service.CreateProjectAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { projectId = project.Id }, project);
    }

    [HttpGet("{projectId:int}")]
    public async Task<IActionResult> Get(int projectId, CancellationToken ct) =>
        Ok(await service.GetProjectAsync(projectId, ct));

    [HttpGet("{projectId:int}/tasks")]
    public async Task<IActionResult> GetTasks(int projectId, CancellationToken ct) =>
        Ok(await service.GetTasksAsync(projectId, ct));

    [HttpPost("{projectId:int}/tasks")]
    public async Task<IActionResult> CreateTask(int projectId, CreateTaskRequest request, CancellationToken ct)
    {
        var task = await service.CreateTaskAsync(projectId, request, ct);
        return CreatedAtAction("Get", "Tasks", new { taskId = task.Id }, task);
    }
}
