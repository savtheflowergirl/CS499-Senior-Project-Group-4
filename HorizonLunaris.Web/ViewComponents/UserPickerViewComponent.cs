using HorizonLunaris.Contracts;
using HorizonLunaris.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace HorizonLunaris.Web.ViewComponents;

public class UserPickerViewComponent(IHorizonApiClient api) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var model = new UserPickerModel
        {
            CurrentUserId = HttpContext.Session.GetInt32(UserHeaderHandler.SessionKey),
            ReturnUrl = Request.Path + Request.QueryString
        };

        try
        {
            model.Users = await api.GetUsersAsync();
        }
        catch (Exception ex) when (ex is HttpRequestException or ApiException)
        {
            model.Error = "API unreachable";
        }

        return View(model);
    }
}

public class UserPickerModel
{
    public IReadOnlyList<UserDto> Users { get; set; } = [];
    public int? CurrentUserId { get; set; }
    public string ReturnUrl { get; set; } = "/";
    public string? Error { get; set; }
}
