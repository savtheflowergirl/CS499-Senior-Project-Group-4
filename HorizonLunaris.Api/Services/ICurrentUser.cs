using HorizonLunaris.Domain.Entities;

namespace HorizonLunaris.Api.Services;

/// <summary>Who is calling? The prototype reads the X-User-Id header (no real login).</summary>
public interface ICurrentUser
{
    /// <summary>Returns the acting user, or throws UnauthorizedAccessException (401).</summary>
    Task<User> GetRequiredAsync(CancellationToken ct = default);

    /// <summary>Returns the acting user if they are a Project Manager, otherwise throws ForbiddenException (403).</summary>
    Task<User> GetRequiredProjectManagerAsync(CancellationToken ct = default);
}
