using System.ComponentModel.DataAnnotations;

namespace HorizonLunaris.Contracts;

// ----- Feature 1: projects -----

public record CreateProjectRequest(
    [param: Required, StringLength(150)] string Name,
    [param: StringLength(2000)] string? Description);

public record ProjectDto(
    int Id,
    string Name,
    string? Description,
    DateTime CreatedAtUtc,
    int CreatedById);
