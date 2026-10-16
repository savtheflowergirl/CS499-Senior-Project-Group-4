using HorizonLunaris.Contracts;
using HorizonLunaris.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonLunaris.Web.Pages;

public class IndexModel(IHorizonApiClient api) : PageModel
{
    public IReadOnlyList<UserDto> Users { get; private set; } = [];
    public int? CurrentUserId { get; private set; }
    public string? Error { get; private set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        CurrentUserId = HttpContext.Session.GetInt32(UserHeaderHandler.SessionKey);
        try
        {
            Users = await api.GetUsersAsync(ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or ApiException)
        {
            Error = $"Could not load users from the API: {ex.Message}";
        }
    }

    public IActionResult OnPost(int userId)
    {
        HttpContext.Session.SetInt32(UserHeaderHandler.SessionKey, userId);
        return RedirectToPage();
    }
}
