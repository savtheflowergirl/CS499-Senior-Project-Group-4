using HorizonLunaris.Contracts;
using HorizonLunaris.Domain.Enums;
using HorizonLunaris.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonLunaris.Web.Pages;

/// <summary>
/// Base class for pages that call the API. Knows who is "acting" (from the sidebar picker / session)
/// and carries one-shot success and error messages across redirects.
/// </summary>
public abstract class HorizonPageModel(IHorizonApiClient api) : PageModel
{
    protected IHorizonApiClient Api { get; } = api;

    public IReadOnlyList<UserDto> Users { get; private set; } = [];
    public UserDto? CurrentUser { get; private set; }
    public bool IsProjectManager => CurrentUser?.Role == UserRole.ProjectManager;

    // Shown once after a POST -> redirect
    [TempData] public string? Message { get; set; }
    [TempData] public string? ErrorMessage { get; set; }

    protected async Task LoadUsersAsync(CancellationToken ct)
    {
        Users = await Api.GetUsersAsync(ct);
        var id = HttpContext.Session.GetInt32(UserHeaderHandler.SessionKey);
        CurrentUser = Users.FirstOrDefault(u => u.Id == id);
    }
}
