using System.ComponentModel.DataAnnotations;

namespace HorizonLunaris.Contracts;

public record EmailSenderSettingsDto(string SourceEmail, bool SmtpConfigured);

public record UpdateEmailSenderRequest(
    [property: Required, EmailAddress, StringLength(254)] string SourceEmail);
