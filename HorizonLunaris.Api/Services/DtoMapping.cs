
using HorizonLunaris.Contracts;
using HorizonLunaris.Domain.Entities;

namespace HorizonLunaris.Api.Services;

/// <summary>
/// Shared entity -> DTO mapping. Task mapping expects Assignees and Prerequisites to be loaded.
/// </summary>
public static class DtoMapping
{
    public static UserDto ToDto(this User u) =>
        new(u.Id, u.Name, u.Email, u.Role);

    public static ProjectDto ToDto(this Project p) =>
        new(p.Id, p.Name, p.Description, p.CreatedAtUtc, p.CreatedById);

    public static TaskDto ToDto(this TaskItem t)
    {
        var assignees = t.Assignees
            .OrderBy(u => u.Name)
            .Select(u => new AssigneeDto(u.Id, u.Name))
            .ToList();

        return new TaskDto(t.Id, t.Title, t.Description, t.Status, t.DueDateUtc, t.CompletedAtUtc,
            t.ProjectId, t.ParentTaskId, assignees.FirstOrDefault()?.Id, assignees.FirstOrDefault()?.Name,
            t.Prerequisites.Select(d => d.PrerequisiteTaskId).ToList(), assignees)
            t.RequiresReview,
            t.IsApproved,
            t.ApprovedAtUtc);
    }

    public static CommentDto ToDto(this Comment c) =>
        new(c.Id, c.TaskItemId, c.AuthorId, c.Author?.Name ?? string.Empty, c.Text, c.CreatedAtUtc);
}
