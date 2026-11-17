using HorizonLunaris.Data;
using HorizonLunaris.Domain.Entities;
using HorizonLunaris.Domain.Enums;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;

namespace HorizonLunaris.Api.Services;

public class EmailNotificationService(
    HorizonLunarisDbContext db,
    IOptions<EmailSmtpOptions> smtpOptions,
    ILogger<EmailNotificationService> logger) : INotificationService
{
    public Task NotifyTaskAvailableAsync(TaskItem task, CancellationToken ct = default) =>
        SendToAssigneesAsync(
            task,
            $"Task available: {task.Title}",
            $"The task \"{task.Title}\" is now available.",
            ct);

    public Task NotifyTaskStatusChangedAsync(
        TaskItem task, TaskItemStatus previousStatus, TaskItemStatus newStatus, CancellationToken ct = default) =>
        SendToAssigneesAsync(
            task,
            $"Task status changed: {task.Title}",
            $"The status of task \"{task.Title}\" changed from {previousStatus} to {newStatus}.",
            ct);

    private async Task SendToAssigneesAsync(TaskItem task, string subject, string body, CancellationToken ct)
    {
        if (task.Assignees.Count == 0)
        {
            return;
        }

        var options = smtpOptions.Value;
        var savedSettings = await db.EmailNotificationSettings.FindAsync([1], ct);
        var sourceEmail = savedSettings?.SourceEmail ?? options.FromEmail;
        if (string.IsNullOrWhiteSpace(options.Host) || string.IsNullOrWhiteSpace(sourceEmail))
        {
            logger.LogWarning("Email notification for task {TaskId} was not sent because SMTP is not configured.", task.Id);
            return;
        }

        if (options.Port is <= 0 or > 65535 ||
            (string.IsNullOrWhiteSpace(options.UserName) != string.IsNullOrWhiteSpace(options.Password)))
        {
            throw new NotificationDeliveryException("SMTP settings are invalid. Check the email configuration in user-secrets or environment variables.");
        }

        var failedRecipients = new List<string>();
        foreach (var assignee in task.Assignees)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(options.FromName, sourceEmail));
                message.To.Add(new MailboxAddress(assignee.Name, assignee.Email));
                message.Subject = subject;
                message.Body = new TextPart("plain") { Text = body };

                using var client = new SmtpClient();
                var socketOptions = options.UseSsl
                    ? SecureSocketOptions.SslOnConnect
                    : SecureSocketOptions.StartTls;
                await client.ConnectAsync(options.Host, options.Port, socketOptions, ct);
                if (!string.IsNullOrWhiteSpace(options.UserName))
                {
                    await client.AuthenticateAsync(options.UserName, options.Password, ct);
                }
                await MailKit.IMailTransportExtensions.SendAsync(client, message, ct);
                await client.DisconnectAsync(true, ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Could not send a task notification email for task {TaskId} to assignee {UserId}.", task.Id, assignee.Id);
                failedRecipients.Add(assignee.Name);
            }
        }

        if (failedRecipients.Count > 0)
        {
            throw new NotificationDeliveryException(
                $"Task status changed, but notification email could not be sent to {failedRecipients.Count} assignee(s). Check the SMTP configuration and API logs.");
        }
    }
}
