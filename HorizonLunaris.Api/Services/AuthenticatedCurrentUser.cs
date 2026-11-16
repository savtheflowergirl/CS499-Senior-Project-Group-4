
using System.Security.Claims;
using HorizonLunaris.Contracts;
using HorizonLunaris.Data;
using HorizonLunaris.Domain.Entities;
using HorizonLunaris.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HorizonLunaris.Api.Services;

public class AuthenticatedCurrentUser(
    IHttpContextAccessor accessor,
    HorizonLunarisDbContext db) : ICurrentUser
{
    private User? _cached;

    public async Task<User> GetRequiredAsync(
        CancellationToken ct = default)
    {
        if (_cached is not null)
            return _cached;

        var principal = accessor.HttpContext?.User;

        if (principal?.Identity?.IsAuthenticated != true)
            throw new UnauthorizedAccessException(
                "You must be logged in.");

        var idClaim = principal.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(idClaim, out var userId))
            throw new UnauthorizedAccessException(
                "Invalid authentication session.");

        _cached = await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new UnauthorizedAccessException(
                "User account no longer exists.");

        return _cached;
    }

    public async Task<User> GetRequiredProjectManagerAsync(
        CancellationToken ct = default)
    {
        var user = await GetRequiredAsync(ct);

        if (user.Role != UserRole.ProjectManager)
            throw new ForbiddenException(
                "Only a Project Manager can perform this action.");

        return user;
    }
}
