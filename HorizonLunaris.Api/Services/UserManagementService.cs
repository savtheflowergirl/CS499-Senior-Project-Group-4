using System.Net.Mail;
using HorizonLunaris.Contracts;
using HorizonLunaris.Data;
using HorizonLunaris.Domain.Entities;
using HorizonLunaris.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HorizonLunaris.Api.Services;

public class UserManagementService(HorizonLunarisDbContext db, ICurrentUser currentUser) : IUserManagementService
{
    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        await currentUser.GetRequiredAdminAsync(ct);

        var name = request.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name) || name.Length > 100)
        {
            throw new BusinessRuleException("Enter a name of 1 to 100 characters.");
        }

        var email = NormalizeEmail(request.Email);
        if (!Enum.IsDefined(request.Role))
        {
            throw new BusinessRuleException("Choose a valid user role.");
        }

        if (await db.Users.AnyAsync(user => user.Email.ToLower() == email, ct))
        {
            throw new BusinessRuleException("A user with that email address already exists.");
        }

        var user = new User { Name = name, Email = email, Role = request.Role };
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);
        return user.ToDto();
    }

    public async Task<UserDto> UpdateEmailAsync(int userId, UpdateUserEmailRequest request, CancellationToken ct = default)
    {
        await currentUser.GetRequiredAdminAsync(ct);

        var user = await db.Users.FirstOrDefaultAsync(candidate => candidate.Id == userId, ct)
            ?? throw new NotFoundException("User not found.");
        var email = NormalizeEmail(request.Email);

        if (await db.Users.AnyAsync(candidate => candidate.Id != userId && candidate.Email.ToLower() == email, ct))
        {
            throw new BusinessRuleException("A user with that email address already exists.");
        }

        user.Email = email;
        await db.SaveChangesAsync(ct);
        return user.ToDto();
    }

    private static string NormalizeEmail(string? value)
    {
        var email = value?.Trim();
        if (email is null || email.Length > 200 ||
            !MailAddress.TryCreate(email, out var address) ||
            !string.Equals(address.Address, email, StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessRuleException("Enter a valid email address of at most 200 characters.");
        }

        return address.Address.ToLowerInvariant();
    }
}
