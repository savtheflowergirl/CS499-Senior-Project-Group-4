using HorizonLunaris.Contracts;
using HorizonLunaris.Domain.Enums;
using HorizonLunaris.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace HorizonLunaris.Web.Pages.Automations;

public class IndexModel(IHorizonApiClient api) : HorizonPageModel(api)
{
    public IReadOnlyList<ProjectDto> Projects { get; private set; } = [];
    public int? SelectedProjectId { get; private set; }
    public IReadOnlyList<TaskDto> Tasks { get; private set; } = [];
    public Dictionary<int, TaskDto> TaskById { get; private set; } = new();
    public string? LoadError { get; private set; }

    // GET /Automations?projectId=2
    public async Task OnGetAsync(int? projectId, CancellationToken ct)
    {
        try
        {
            await LoadUsersAsync(ct);
            Projects = await Api.GetProjectsAsync(ct);
            SelectedProjectId = projectId ?? Projects.FirstOrDefault()?.Id;

            if (SelectedProjectId is { } id)
            {
                Tasks = await Api.GetTasksAsync(id, ct);
                TaskById = Tasks.ToDictionary(t => t.Id);
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or ApiException)
        {
            LoadError = $"Could not load dependencies: {ex.Message}";
        }
    }

    public async Task<IActionResult> OnPostAddDependencyAsync(int projectId, int taskId, int prerequisiteTaskId, CancellationToken ct)
    {
        try
        {
            var titles = await TitlesAsync(projectId, ct);
            var task = await Api.AddDependencyAsync(taskId, new AddDependencyRequest(prerequisiteTaskId), ct);
            Message = $"\"{task.Title}\" now depends on \"{NameOf(titles, prerequisiteTaskId)}\".";

            if (task.Status == TaskItemStatus.Blocked)
            {
                Message += " It is Blocked until its prerequisites are completed.";
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or ApiException)
        {
            // e.g. "The dependency would create a circular dependency."
            ErrorMessage = ex.Message;
        }
        return RedirectToPage(new { projectId });
    }

    public async Task<IActionResult> OnPostRemoveDependencyAsync(int projectId, int taskId, int prerequisiteTaskId, CancellationToken ct)
    {
        try
        {
            var titles = await TitlesAsync(projectId, ct);   // look up names before the dependency is removed
            await Api.RemoveDependencyAsync(taskId, prerequisiteTaskId, ct);
            Message = $"Removed dependency: \"{NameOf(titles, taskId)}\" no longer waits for \"{NameOf(titles, prerequisiteTaskId)}\".";
        }
        catch (Exception ex) when (ex is HttpRequestException or ApiException)
        {
            ErrorMessage = ex.Message;
        }
        return RedirectToPage(new { projectId });
    }

    private async Task<Dictionary<int, string>> TitlesAsync(int projectId, CancellationToken ct) =>
        (await Api.GetTasksAsync(projectId, ct)).ToDictionary(t => t.Id, t => t.Title);

    private static string NameOf(Dictionary<int, string> titles, int taskId) =>
        titles.TryGetValue(taskId, out var title) ? title : "another task";
}
