using HorizonLunaris.Contracts;

namespace HorizonLunaris.Web.Services;

/// <summary>Adds the X-User-Id header for the user chosen on the home page (stored in session).</summary>
public class UserHeaderHandler(IHttpContextAccessor accessor) : DelegatingHandler
{
    public const string SessionKey = "UserId";

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var userId = accessor.HttpContext?.Session.GetInt32(SessionKey);
        if (userId is not null)
        {
            request.Headers.Add(ApiHeaders.UserId, userId.Value.ToString());
        }
        return base.SendAsync(request, cancellationToken);
    }
}
