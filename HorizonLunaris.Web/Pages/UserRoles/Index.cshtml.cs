using System.Text.RegularExpressions;
using HorizonLunaris.Contracts;
using HorizonLunaris.Domain.Enums;
using HorizonLunaris.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace HorizonLunaris.Web.Pages.UserRoles;

public class IndexModel(IHorizonApiClient api) : HorizonPageModel(api)
{
    private readonly IHorizonApiClient _api = api;

    // GET /UserRoles?Search=ell
    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    public List<TeamMemberRow> Members { get; private set; } = new();
    [BindProperty] public string NewName { get; set; } = "";
    [BindProperty] public string NewEmail { get; set; } = "";
    [BindProperty] public UserRole NewRole { get; set; } = UserRole.TeamMember;

    // The user chosen in the sidebar picker (same session key the API client uses)
    public int? CurrentUserId => HttpContext.Session.GetInt32(UserHeaderHandler.SessionKey);

    public async Task OnGetAsync(CancellationToken ct)
    {
        try
        {
            await LoadUsersAsync(ct);
            var users = Users;

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

    public async Task<IActionResult> OnPostCreateUserAsync(CancellationToken ct)
    {
        try
        {
            var user = await _api.CreateUserAsync(new CreateUserRequest(NewName, NewEmail, NewRole), ct);
            Message = $"Created {user.Name} ({user.Email}).";
        }
        catch (Exception ex) when (ex is HttpRequestException or ApiException)
        {
            ErrorMessage = ex.Message;
        }

        return RedirectToPage(new { Search });
    }

    public async Task<IActionResult> OnPostUpdateEmailAsync(int userId, string email, CancellationToken ct)
    {
        try
        {
            var user = await _api.UpdateUserEmailAsync(userId, new UpdateUserEmailRequest(email), ct);
            Message = $"Updated {user.Name}'s email address.";
        }
        catch (Exception ex) when (ex is HttpRequestException or ApiException)
        {
            ErrorMessage = ex.Message;
        }

        return RedirectToPage(new { Search });
    }

    /// <summary>
    /// Active Tasks = uncompleted tasks assigned to the user.
    /// If this lookup fails, the table still renders and the column shows a dash.
    /// </summary>
    private async Task LoadActiveTaskCountsAsync(CancellationToken ct)
    {
        try
        {
            var projects = await _api.GetProjectsAsync(ct);

            // One call per project (fine for a prototype; an API "tasks by assignee" endpoint would be better later)
            var taskLists = await Task.WhenAll(projects.Select(p => _api.GetTasksAsync(p.Id, ct)));

            var countsByAssignee = taskLists
                .SelectMany(list => list)
                .Where(t => t.CompletedAtUtc is null)
                .SelectMany(t => t.Assignees.Select(assignee => assignee.Id))
                .GroupBy(userId => userId)
                .ToDictionary(g => g.Key, g => g.Count());

            foreach (var m in Members)
            {
                m.ActiveTasks = countsByAssignee.TryGetValue(m.Id, out var n) ? n : 0;
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
