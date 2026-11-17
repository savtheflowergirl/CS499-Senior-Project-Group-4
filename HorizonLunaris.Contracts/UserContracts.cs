using System.ComponentModel.DataAnnotations;
using HorizonLunaris.Domain.Enums;

namespace HorizonLunaris.Contracts;

public record UserDto(int Id, string Name, string Email, UserRole Role);

public record CreateUserRequest(
    [param: Required, StringLength(100)] string Name,
    [param: Required, EmailAddress, StringLength(200)] string Email,
    [param: EnumDataType(typeof(UserRole))] UserRole Role);

public record UpdateUserEmailRequest(
    [param: Required, EmailAddress, StringLength(200)] string Email);
