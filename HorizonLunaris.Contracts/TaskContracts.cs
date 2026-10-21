using System.ComponentModel.DataAnnotations;
using HorizonLunaris.Domain.Enums;

namespace HorizonLunaris.Contracts;

// ----- Feature 1: tasks and ownership -----

public record CreateTaskRequest(
    [param: Required, StringLength(200)] string Title,
    [param: StringLength(4000)] string? Description,
    DateTime? DueDateUtc,
    int? ParentTaskId,
    int? OwnerId);

public record AssignOwnerRequest(int OwnerId);

public record TaskDto(
    int Id,
    string Title,
    string? Description,
    TaskItemStatus Status,
    DateTime? DueDateUtc,
    DateTime? CompletedAtUtc,
    int ProjectId,
    int? ParentTaskId,
    int? OwnerId,
    string? OwnerName,
    IReadOnlyList<int> PrerequisiteTaskIds);

// ----- Feature 2: status updates -----

public record UpdateStatusRequest(TaskItemStatus Status);

/// <summary>The updated task, plus the ids of any tasks that became Available as a result.</summary>
public record StatusUpdateResult(TaskDto Task, IReadOnlyList<int> UnlockedTaskIds);
