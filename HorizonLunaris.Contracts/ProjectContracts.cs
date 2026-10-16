using System.ComponentModel.DataAnnotations;

namespace HorizonLunaris.Contracts;

// ----- Feature 1: projects -----

public record CreateProjectRequest(
    [property: Required, StringLength(150)] string Name,
    [property: StringLength(2000)] string? Description);

public record ProjectDto(
    int Id,
    string Name,
    string? Description,
    DateTime CreatedAtUtc,
    int CreatedById);
