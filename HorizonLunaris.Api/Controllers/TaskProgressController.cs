using HorizonLunaris.Api.Services;
using HorizonLunaris.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace HorizonLunaris.Api.Controllers;

/// <summary>FEATURE 2: status updates and progress comments.</summary>
[ApiController]
[Route("api/tasks/{taskId:int}")]
public class TaskProgressController(ITaskProgressService service) : ControllerBase
{
    [HttpPut("status")]
    public async Task<IActionResult> UpdateStatus(int taskId, UpdateStatusRequest request, CancellationToken ct) =>
        Ok(await service.UpdateStatusAsync(taskId, request, ct));

    [HttpGet("comments")]
    public async Task<IActionResult> GetComments(int taskId, CancellationToken ct) =>
        Ok(await service.GetCommentsAsync(taskId, ct));

    [HttpPost("comments")]
    public async Task<IActionResult> AddComment(int taskId, AddCommentRequest request, CancellationToken ct) =>
        Ok(await service.AddCommentAsync(taskId, request, ct));
}
