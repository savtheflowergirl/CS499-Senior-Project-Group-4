using HorizonLunaris.Contracts;
using HorizonLunaris.Domain.Enums;
using HorizonLunaris.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;


namespace HorizonLunaris.Web.Pages;
public class ProjectsModel(IHorizonApiClient api) : PageModel
{
    public IReadOnlyList<ProjectDto> Projects { get; set; } = [];
    public Dictionary<int, IReadOnlyList<CommentDto>> TaskComments { get; set; } = new();
    public IReadOnlyList<TaskDto> Tasks { get; set; } = [];
    public IReadOnlyList<UserDto> Users { get; set; } = [];

    public int? SelectedProjectId { get; set; }
    public string? Error { get; set; }
    // load project,task,users;SM
    public async Task OnGetAsync(int? projectId, CancellationToken ct)
    {
        SelectedProjectId = projectId;
        try
        {
            Projects = await api.GetProjectsAsync(ct);
            Users = await api.GetUsersAsync(ct);
            if (projectId.HasValue)
            {
               Tasks = await api.GetTasksAsync(projectId.Value, ct);
                   foreach (var task in Tasks)
                   {
                       TaskComments[task.Id] = await api.GetCommentsAsync(task.Id, ct);
                   }
               }
        }
        catch (Exception ex) when (ex is ApiException or HttpRequestException)
        {
            Error = ex.Message;
        }
    }
    // create a new project; SM
    public async Task<IActionResult> OnPostCreateProjectAsync(
        string name, string? description)
    {
        try
        {
            var project = await api.CreateProjectAsync(
                new CreateProjectRequest(name, description));
            return RedirectToPage(new { projectId = project.Id });
        }
        catch (Exception ex) when (ex is ApiException or HttpRequestException)
        {
            var message = ex.Message;
            await OnGetAsync(null, HttpContext.RequestAborted);
            Error = message;
            return Page();
        }
    }
    // creatse a task or subtask; SM
    public async Task<IActionResult> OnPostCreateTaskAsync(
        int projectId, string title, string? description,
        int? parentTaskId, int? ownerId)
    {
        try
        {
            var request = new CreateTaskRequest(
                title, description, null, parentTaskId, ownerId);
            await api.CreateTaskAsync(projectId, request);
            return RedirectToPage(new { projectId });
        }
        catch (Exception ex) when (ex is ApiException or HttpRequestException)
        {
            var message = ex.Message;
            await OnGetAsync(projectId, HttpContext.RequestAborted);
            Error = message;
            return Page();
        }
    }
    // assign/reassign a task to user, SM
    public async Task<IActionResult> OnPostAssignOwnerAsync(
        int projectId, int taskId, int ownerId)
    {
        try
        {
            await api.AssignOwnerAsync(taskId, new AssignOwnerRequest(ownerId));
            return RedirectToPage(new { projectId });
        }
        catch (Exception ex) when (ex is ApiException or HttpRequestException)
        {
            var message = ex.Message;
            await OnGetAsync(projectId, HttpContext.RequestAborted);
            Error = message;
            return Page();
        }
    }
    // update task status; SM
    public async Task<IActionResult> OnPostUpdateStatusAsync(
        int projectId, int taskId, TaskItemStatus status)
    {
        try
        {
            await api.UpdateStatusAsync(
                taskId, new UpdateStatusRequest(status));

            return RedirectToPage(new { projectId });
        }
        catch (Exception ex) when (ex is ApiException or HttpRequestException)
        {
            var message = ex.Message;
            await OnGetAsync(projectId, HttpContext.RequestAborted);
            Error = message;
            return Page();
        }
    }
    public async Task<IActionResult> OnPostAddCommentAsync(
        int projectId, int taskId, string text)
    {
        try
        {
            await api.AddCommentAsync(taskId, new AddCommentRequest(text));
            return RedirectToPage(new { projectId });
        }
        catch (Exception ex) when (ex is ApiException or HttpRequestException)
        {
            var message = ex.Message;
            await OnGetAsync(projectId, HttpContext.RequestAborted);
            Error = message;
            return Page();
        }
    }

}
