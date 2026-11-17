using System.ComponentModel.DataAnnotations;
using System.Globalization;
using HorizonLunaris.Contracts;
using HorizonLunaris.Domain.Enums;
using HorizonLunaris.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace HorizonLunaris.Web.Pages.Tasks;

public class ProjectModel(IHorizonApiClient api) : HorizonPageModel(api)
{
    public ProjectDto? Project { get; private set; }
    public IReadOnlyList<TaskDto> Tasks { get; private set; } = [];
    public List<TaskRow> Rows { get; private set; } = [];
    public Dictionary<int, TaskDto> TaskById { get; private set; } = new();
    public Dictionary<int, IReadOnlyList<CommentDto>> Comments { get; private set; } = new();
    public string? LoadError { get; private set; }

    // ?open=5 keeps that task's comments expanded after posting a comment
    [BindProperty(SupportsGet = true)]
    public int? Open { get; set; }

    [BindProperty]
    public NewTaskInput NewTask { get; set; } = new();

    // Statuses a person can pick by hand. Blocked is set by the system, never chosen.
    public static readonly TaskItemStatus[] StatusChoices =
        [TaskItemStatus.Available, TaskItemStatus.InProgress, TaskItemStatus.Completed];

    /// <summary>Task name for an id, so the UI never has to show raw database numbers.</summary>
    public string TitleOf(int taskId) =>
        TaskById.TryGetValue(taskId, out var task) ? task.Title : "Unknown task";

    /// <summary>Mirrors the API rule: only the task owner or a Project Manager may change status.</summary>
    public bool CanChangeStatus(TaskDto task) =>
        CurrentUser is not null && (IsProjectManager || task.Assignees.Any(assignee => assignee.Id == CurrentUser.Id));

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        try
        {
            await LoadUsersAsync(ct);
            Project = await Api.GetProjectAsync(id, ct);
            Tasks = await Api.GetTasksAsync(id, ct);
            Rows = Flatten(Tasks);
            TaskById = Tasks.ToDictionary(t => t.Id);

            // Comments for every task in the project (one call per task)
            var commentLists = await Task.WhenAll(Tasks.Select(t => Api.GetCommentsAsync(t.Id, ct)));
            for (var i = 0; i < Tasks.Count; i++)
            {
                Comments[Tasks[i].Id] = commentLists[i];
            }
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

    // ----- Feature 1 -----

    public async Task<IActionResult> OnPostCreateTaskAsync(int id, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return await OnGetAsync(id, ct);
        }

        if (string.IsNullOrWhiteSpace(NewTask.Title))
        {
            ErrorMessage = "Task title is required.";
            return RedirectToPage(new { id });
        }

        try
        {
            var description = string.IsNullOrWhiteSpace(NewTask.Description) ? null : NewTask.Description.Trim();
            DateTime? dueUtc = null;
            if (!string.IsNullOrEmpty(NewTask.DueDate))
            {
                var dueDate = DateOnly.ParseExact(NewTask.DueDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None);
                dueUtc = DateTime.SpecifyKind(dueDate.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
            }

            var task = await Api.CreateTaskAsync(id,
                new CreateTaskRequest(NewTask.Title.Trim(), description, dueUtc, NewTask.ParentTaskId, null, NewTask.AssigneeIds), ct);

            Message = $"Created task \"{task.Title}\".";
        }
        catch (Exception ex) when (ex is HttpRequestException or ApiException)
        {
            ErrorMessage = ex.Message;
        }
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostAssignAssigneesAsync(int id, int taskId, int[]? assigneeIds, CancellationToken ct)
    {
        try
        {
            var task = await Api.AssignAssigneesAsync(taskId, new AssignAssigneesRequest(assigneeIds ?? []), ct);
            var names = task.Assignees.Count == 0 ? "no one" : string.Join(", ", task.Assignees.Select(assignee => assignee.Name));
            Message = $"Updated assignments for \"{task.Title}\": {names}.";
        }
        catch (Exception ex) when (ex is HttpRequestException or ApiException)
        {
            ErrorMessage = ex.Message;
        }
        return RedirectToPage(new { id });
    }

    // Edit an existing task; SM
    public async Task<IActionResult> OnPostEditTaskAsync(
        int id,
        int taskId,
        string title,
        string? description,
        DateTime? dueDate,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            ErrorMessage = "Task title is required.";
            return RedirectToPage(new { id });
        }

        try
        {
            DateTime? dueUtc = dueDate is { } d
                ? DateTime.SpecifyKind(d.Date, DateTimeKind.Utc)
                : null;

            var request = new UpdateTaskRequest(
                title.Trim(),
                string.IsNullOrWhiteSpace(description)
                    ? null
                    : description.Trim(),
                dueUtc);

            var task = await Api.UpdateTaskAsync(
                taskId, request, ct);

            Message = $"Task \"{task.Title}\" updated successfully.";
        }
        catch (Exception ex) when (
            ex is HttpRequestException or ApiException)
        {
            ErrorMessage = ex.Message;
        }

        return RedirectToPage(new { id });
    }

    // Delete task using the selected chain deletion option; SM
    public async Task<IActionResult> OnPostDeleteTaskAsync(
        int id,
        int taskId,
        TaskDeletionMode deletionMode,
        CancellationToken ct)
    {
        try
        {
            // Send the selected deletion option to the API; SM
            var request = new DeleteTaskRequest(deletionMode);

            await Api.DeleteTaskAsync(taskId, request, ct);

            Message = deletionMode switch
            {
                TaskDeletionMode.DeleteChildren =>
                    "Task and all its subtasks deleted successfully.",

                TaskDeletionMode.KeepChildren =>
                    "Task deleted successfully. Its subtasks were preserved.",

                _ => "Task deleted successfully."
            };
        }
        catch (Exception ex) when (
            ex is HttpRequestException or ApiException)
        {
            ErrorMessage = ex.Message;
        }

        return RedirectToPage(new { id });
    }

    // ----- Feature 2 -----

    public async Task<IActionResult> OnPostUpdateStatusAsync(int id, int taskId, TaskItemStatus status, CancellationToken ct)
    {
        try
        {
            var result = await Api.UpdateStatusAsync(taskId, new UpdateStatusRequest(status), ct);
            Message = $"\"{result.Task.Title}\" is now {Label(result.Task.Status)}.";

            if (result.UnlockedTaskIds.Count > 0)
            {
                // The API returns ids; look up the names so the banner reads naturally
                var titles = (await Api.GetTasksAsync(id, ct)).ToDictionary(t => t.Id, t => t.Title);
                var names = result.UnlockedTaskIds.Select(i => titles.TryGetValue(i, out var title) ? $"\"{title}\"" : "another task");
                Message += " Unlocked: " + string.Join(", ", names) + ".";
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or ApiException)
        {
            // e.g. "A blocked task cannot be started or completed." / "You cannot change this task's status."
            ErrorMessage = ex.Message;
        }
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostAddCommentAsync(int id, int taskId, string text, CancellationToken ct)
    {
        try
        {
            await Api.AddCommentAsync(taskId, new AddCommentRequest(text ?? ""), ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or ApiException)
        {
            ErrorMessage = ex.Message;
        }
        // Come back with that task's comments open
        return RedirectToPage(pageName: null, pageHandler: null, routeValues: new { id, open = taskId }, fragment: $"task-{taskId}");
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

        [CustomValidation(typeof(NewTaskInput), nameof(ValidateDueDate))]
        public string? DueDate { get; set; }

        public int? ParentTaskId { get; set; }
        public List<int> AssigneeIds { get; set; } = [];

        public static ValidationResult? ValidateDueDate(string? value, ValidationContext context)
        {
            if (string.IsNullOrEmpty(value) || DateOnly.TryParseExact(
                    value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult("Enter a valid date in YYYY-MM-DD format.");
        }
    }
}