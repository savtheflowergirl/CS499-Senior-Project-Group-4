using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace HorizonLunaris.Api.Services;

public class ApiExceptionHandler(
    ILogger<ApiExceptionHandler> logger,
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Not found"),
            ForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden"),
            BusinessRuleException => (StatusCodes.Status409Conflict, "Business rule violated"),
            NotificationDeliveryException => (StatusCodes.Status503ServiceUnavailable, "Email notification delivery failed"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            NotImplementedException => (StatusCodes.Status501NotImplemented, "Not implemented yet"),
            _ => (StatusCodes.Status500InternalServerError, "Server error")
        };

        if (status == StatusCodes.Status500InternalServerError || exception is NotificationDeliveryException)
        {
            logger.LogError(exception, "Request failed while processing email notifications.");
        }

        httpContext.Response.StatusCode = status;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = status == StatusCodes.Status500InternalServerError && exception is not NotificationDeliveryException
                    ? null
                    : exception.Message
            }
        });
    }
}
