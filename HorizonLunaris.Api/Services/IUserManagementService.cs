using HorizonLunaris.Contracts;

namespace HorizonLunaris.Api.Services;

public interface IUserManagementService
{
    Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken ct = default);
    Task<UserDto> UpdateEmailAsync(int userId, UpdateUserEmailRequest request, CancellationToken ct = default);
}
