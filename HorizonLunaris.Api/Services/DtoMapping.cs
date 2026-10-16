using HorizonLunaris.Contracts;
using HorizonLunaris.Domain.Entities;

namespace HorizonLunaris.Api.Services;

/// <summary>
/// Shared entity -> DTO mapping. ADD methods here, do not change existing ones without telling the team.
/// ToDto(TaskItem) expects Owner and Prerequisites to be loaded (use .Include).
/// </summary>
public static class DtoMapping
{
    public static UserDto ToDto(this User u) => new(u.Id, u.Name, u.Email, u.Role);

    public static ProjectDto ToDto(this Project p) =>
        new(p.Id, p.Name, p.Description, p.CreatedAtUtc, p.CreatedById);

    public static TaskDto ToDto(this TaskItem t) =>
        new(t.Id, t.Title, t.Description, t.Status, t.DueDateUtc, t.CompletedAtUtc,
            t.ProjectId, t.ParentTaskId, t.OwnerId, t.Owner?.Name,
            t.Prerequisites.Select(d => d.PrerequisiteTaskId).ToList());

    public static CommentDto ToDto(this Comment c) =>
        new(c.Id, c.TaskItemId, c.AuthorId, c.Author?.Name ?? string.Empty, c.Text, c.CreatedAtUtc);
}
