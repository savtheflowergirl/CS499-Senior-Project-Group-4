using HorizonLunaris.Contracts;
using HorizonLunaris.Data;
using HorizonLunaris.Domain.Entities;
using HorizonLunaris.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HorizonLunaris.Api.Services;

public class HeaderCurrentUser(IHttpContextAccessor accessor, HorizonLunarisDbContext db) : ICurrentUser
{
    private User? _cached;

    public async Task<User> GetRequiredAsync(CancellationToken ct = default)
    {
        if (_cached is not null) return _cached;

        var header = accessor.HttpContext?.Request.Headers[ApiHeaders.UserId].ToString();
        if (!int.TryParse(header, out var userId))
            throw new UnauthorizedAccessException($"Missing or invalid {ApiHeaders.UserId} header.");

        _cached = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, ct)
                  ?? throw new UnauthorizedAccessException($"Unknown user id {userId}.");
        return _cached;
    }

    public async Task<User> GetRequiredProjectManagerAsync(CancellationToken ct = default)
    {
        var user = await GetRequiredAsync(ct);
        if (user.Role != UserRole.ProjectManager)
            throw new ForbiddenException("Only a Project Manager can perform this action.");
        return user;
    }
}
