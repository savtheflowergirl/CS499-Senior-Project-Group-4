using HorizonLunaris.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonLunaris.Web.Pages;

public class SwitchUserModel : PageModel
{
    // Visiting /SwitchUser directly just goes home.
    public IActionResult OnGet() => RedirectToPage("/Index");

    public IActionResult OnPost(int userId, string? returnUrl)
    {
        // Same session key UserHeaderHandler reads, so every API call carries the new user.
        if (userId > 0)
        {
            HttpContext.Session.SetInt32(UserHeaderHandler.SessionKey, userId);
        }

        // Only follow local URLs (prevents open-redirect abuse).
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }
        return RedirectToPage("/Index");
    }
}
