using HorizonLunaris.Domain.Enums;

namespace HorizonLunaris.Contracts;

public record UserDto(int Id, string Name, string Email, UserRole Role);
