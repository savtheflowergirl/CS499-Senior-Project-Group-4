using System.Text.RegularExpressions;
using HorizonLunaris.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonLunaris.Web.Pages.UserRoles;

public class IndexModel : PageModel
{
    private readonly IHorizonApiClient _api;

    public IndexModel(IHorizonApiClient api) => _api = api;

    // GET /UserRoles?Search=ell
    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    public List<TeamMemberRow> Members { get; private set; } = new();

    // The user chosen in the sidebar picker (same session key the API client uses)
    public int? CurrentUserId => HttpContext.Session.GetInt32(UserHeaderHandler.SessionKey);

    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        try
        {
            var users = await _api.GetUsersAsync(ct);

            Members = users.Select(u => new TeamMemberRow
            {
                Id    = u.Id,
                Name  = u.Name,
                Email = u.Email,
                // "ProjectManager" -> "Project Manager"
                Role  = Regex.Replace(u.Role.ToString(), "(?<!^)([A-Z])", " $1")
            }).ToList();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Covers ApiException (API returned an error) and HttpRequestException (API unreachable)
            ErrorMessage = $"Could not load team members: {ex.Message}";
            return;
        }

        await LoadActiveTaskCountsAsync(ct);

        if (!string.IsNullOrWhiteSpace(Search))
        {
            Members = Members.Where(m =>
                m.Name.Contains(Search, StringComparison.OrdinalIgnoreCase) ||
                m.Email.Contains(Search, StringComparison.OrdinalIgnoreCase) ||
                m.Role.Contains(Search, StringComparison.OrdinalIgnoreCase)).ToList();
        }
    }

    /// <summary>
    /// Active Tasks = tasks owned by the user that have not been completed (CompletedAtUtc is null).
    /// If this lookup fails, the table still renders and the column shows a dash.
    /// </summary>
    private async Task LoadActiveTaskCountsAsync(CancellationToken ct)
    {
        try
        {
            var projects = await _api.GetProjectsAsync(ct);

            // One call per project (fine for a prototype; an API "tasks by owner" endpoint would be better later)
            var taskLists = await Task.WhenAll(projects.Select(p => _api.GetTasksAsync(p.Id, ct)));

            var countsByOwner = taskLists
                .SelectMany(list => list)
                .Where(t => t.OwnerId.HasValue && t.CompletedAtUtc is null)
                .GroupBy(t => t.OwnerId!.Value)
                .ToDictionary(g => g.Key, g => g.Count());

            foreach (var m in Members)
            {
                m.ActiveTasks = countsByOwner.TryGetValue(m.Id, out var n) ? n : 0;
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Leave ActiveTasks as null so the view shows a dash
        }
    }

    public class TeamMemberRow
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Role { get; set; } = "";
        public int? ActiveTasks { get; set; }
    }
}
