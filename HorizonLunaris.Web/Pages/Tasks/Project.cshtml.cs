using HorizonLunaris.Contracts;
using HorizonLunaris.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace HorizonLunaris.Web.Pages.Tasks;

public class ProjectModel(IHorizonApiClient api) : HorizonPageModel(api)
{
    public ProjectDto? Project { get; private set; }
    public IReadOnlyList<TaskDto> Tasks { get; private set; } = [];
    public List<TaskRow> Rows { get; private set; } = [];
    public string? LoadError { get; private set; }

    [BindProperty]
    public NewTaskInput NewTask { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        try
        {
            await LoadUsersAsync(ct);
            Project = await Api.GetProjectAsync(id, ct);
            Tasks = await Api.GetTasksAsync(id, ct);
            Rows = Flatten(Tasks);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return NotFound();
        }
        catch (Exception ex) when (ex is HttpRequestException or ApiException)
        {
            LoadError = $"Could not load this project: {ex.Message}";
        }
        return Page();
    }

    public async Task<IActionResult> OnPostCreateTaskAsync(int id, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(NewTask.Title))
        {
            ErrorMessage = "Task title is required.";
            return RedirectToPage(new { id });
        }

        try
        {
            var description = string.IsNullOrWhiteSpace(NewTask.Description) ? null : NewTask.Description.Trim();
            DateTime? dueUtc = NewTask.DueDate is { } d ? DateTime.SpecifyKind(d.Date, DateTimeKind.Utc) : null;

            var task = await Api.CreateTaskAsync(id,
                new CreateTaskRequest(NewTask.Title.Trim(), description, dueUtc, NewTask.ParentTaskId, NewTask.OwnerId), ct);

            Message = $"Created task \"{task.Title}\".";
        }
        catch (Exception ex) when (ex is HttpRequestException or ApiException)
        {
            ErrorMessage = ex.Message;
        }
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostAssignOwnerAsync(int id, int taskId, int ownerId, CancellationToken ct)
    {
        try
        {
            var task = await Api.AssignOwnerAsync(taskId, new AssignOwnerRequest(ownerId), ct);
            Message = $"Assigned \"{task.Title}\" to {task.OwnerName}.";
        }
        catch (Exception ex) when (ex is HttpRequestException or ApiException)
        {
            ErrorMessage = ex.Message;
        }
        return RedirectToPage(new { id });
    }

    /// <summary>Orders tasks so each sub-task appears directly under its parent, with an indent depth.</summary>
    private static List<TaskRow> Flatten(IReadOnlyList<TaskDto> tasks)
    {
        var ids = tasks.Select(t => t.Id).ToHashSet();
        var childrenOf = tasks
            .Where(t => t.ParentTaskId.HasValue && ids.Contains(t.ParentTaskId.Value))
            .ToLookup(t => t.ParentTaskId!.Value);
        var roots = tasks.Where(t => !t.ParentTaskId.HasValue || !ids.Contains(t.ParentTaskId.Value));

        var result = new List<TaskRow>();
        void Add(TaskDto task, int depth)
        {
            result.Add(new TaskRow(task, depth));
            foreach (var child in childrenOf[task.Id]) Add(child, depth + 1);
        }
        foreach (var root in roots) Add(root, 0);
        return result;
    }

    public record TaskRow(TaskDto Task, int Depth);

    public class NewTaskInput
    {
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public int? ParentTaskId { get; set; }
        public int? OwnerId { get; set; }
    }
}
