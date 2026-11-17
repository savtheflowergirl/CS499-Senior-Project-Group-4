using HorizonLunaris.Contracts;
using HorizonLunaris.Data;
using HorizonLunaris.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net.Mail;

namespace HorizonLunaris.Api.Services;

public class EmailSenderSettingsService(
    HorizonLunarisDbContext db,
    ICurrentUser currentUser,
    IOptions<EmailSmtpOptions> smtpOptions) : IEmailSenderSettingsService
{
    public async Task<EmailSenderSettingsDto> GetAsync(CancellationToken ct = default)
    {
        var savedSettings = await db.EmailNotificationSettings.FindAsync([1], ct);
        var sourceEmail = savedSettings?.SourceEmail ?? smtpOptions.Value.FromEmail;
        return new EmailSenderSettingsDto(sourceEmail, IsSmtpConfigured(sourceEmail));
    }

    public async Task<EmailSenderSettingsDto> UpdateAsync(string sourceEmail, CancellationToken ct = default)
    {
        await currentUser.GetRequiredProjectManagerAsync(ct);

        if (!MailAddress.TryCreate(sourceEmail, out var address) ||
            !string.Equals(address.Address, sourceEmail, StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessRuleException("Enter a valid source email address.");
        }

        var settings = await db.EmailNotificationSettings.FindAsync([1], ct);
        if (settings is null)
        {
            settings = new EmailNotificationSettings { Id = 1, SourceEmail = address.Address };
            db.EmailNotificationSettings.Add(settings);
        }
        else
        {
            settings.SourceEmail = address.Address;
        }

        await db.SaveChangesAsync(ct);
        return new EmailSenderSettingsDto(settings.SourceEmail, IsSmtpConfigured(settings.SourceEmail));
    }

    private bool IsSmtpConfigured(string sourceEmail)
    {
        var options = smtpOptions.Value;
        return !string.IsNullOrWhiteSpace(sourceEmail) &&
               !string.IsNullOrWhiteSpace(options.Host) &&
               options.Port is > 0 and <= 65535 &&
               (string.IsNullOrWhiteSpace(options.UserName) == string.IsNullOrWhiteSpace(options.Password));
    }
}
