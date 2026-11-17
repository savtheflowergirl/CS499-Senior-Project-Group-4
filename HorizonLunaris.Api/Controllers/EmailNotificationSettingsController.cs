using HorizonLunaris.Api.Services;
using HorizonLunaris.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace HorizonLunaris.Api.Controllers;

[ApiController]
[Route("api/notifications/email-settings")]
public class EmailNotificationSettingsController(IEmailSenderSettingsService settingsService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct) =>
        Ok(await settingsService.GetAsync(ct));

    [HttpPut]
    public async Task<IActionResult> Update(UpdateEmailSenderRequest request, CancellationToken ct) =>
        Ok(await settingsService.UpdateAsync(request.SourceEmail, ct));
}
