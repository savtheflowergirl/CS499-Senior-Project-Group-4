using HorizonLunaris.Contracts;

namespace HorizonLunaris.Api.Services;

public interface IEmailSenderSettingsService
{
    Task<EmailSenderSettingsDto> GetAsync(CancellationToken ct = default);
    Task<EmailSenderSettingsDto> UpdateAsync(string sourceEmail, CancellationToken ct = default);
}
