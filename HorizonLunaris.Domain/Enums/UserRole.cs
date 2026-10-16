namespace HorizonLunaris.Domain.Enums;

/// <summary>
/// Role used by the prototype's permission checks.
/// Only ProjectManager may create or modify task dependencies.
/// </summary>
public enum UserRole
{
    TeamMember = 0,
    ProjectManager = 1
}
