using HorizonLunaris.Contracts;
using HorizonLunaris.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace HorizonLunaris.Web.Pages.Tasks;

public class IndexModel(IHorizonApiClient api) : HorizonPageModel(api)
{
    public IReadOnlyList<ProjectDto> Projects { get; private set; } = [];
    public string? LoadError { get; private set; }

    [BindProperty]
    public NewProjectInput NewProject { get; set; } = new();

    public async Task OnGetAsync(CancellationToken ct)
    {
        try
        {
            await LoadUsersAsync(ct);
            Projects = await Api.GetProjectsAsync(ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or ApiException)
        {
            LoadError = $"Could not load projects: {ex.Message}";
        }
    }

    public async Task<IActionResult> OnPostCreateProjectAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(NewProject.Name))
        {
            ErrorMessage = "Project name is required.";
            return RedirectToPage();
        }

        try
        {
            var description = string.IsNullOrWhiteSpace(NewProject.Description) ? null : NewProject.Description.Trim();
            var project = await Api.CreateProjectAsync(new CreateProjectRequest(NewProject.Name.Trim(), description), ct);
            Message = $"Created project \"{project.Name}\".";
            return RedirectToPage("/Tasks/Project", new { id = project.Id });
        }
        catch (Exception ex) when (ex is HttpRequestException or ApiException)
        {
            // e.g. "Only a Project Manager can perform this action."
            ErrorMessage = ex.Message;
            return RedirectToPage();
        }
    }

    public class NewProjectInput
    {
        public string Name { get; set; } = "";
        public string? Description { get; set; }
    }
}
