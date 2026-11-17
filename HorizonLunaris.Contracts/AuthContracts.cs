namespace HorizonLunaris.Contracts;

public record LoginRequest(string Email, string Password);

public record LoginResponse(
    int UserId,
    string Name,
    string Email,
    string Role
);
