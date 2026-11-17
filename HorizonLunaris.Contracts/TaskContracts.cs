using HorizonLunaris.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace HorizonLunaris.Contracts;

// ----- Feature 1: tasks and assignments -----

public record CreateTaskRequest(
    [param: Required, StringLength(200)] string Title,
    [param: StringLength(4000)] string? Description,
    DateTime? DueDateUtc,
    int? ParentTaskId,
    int? OwnerId,
    IReadOnlyList<int>? AssigneeIds = null);

public record UpdateTaskRequest(
    [param: Required, StringLength(200)] string Title,
    [param: StringLength(4000)] string? Description,
    DateTime? DueDateUtc);

// ----- Chain deletion options; SM -----

public enum TaskDeletionMode
{
    TaskOnly,
    DeleteChildren,
    KeepChildren
}

public record DeleteTaskRequest(TaskDeletionMode Mode);

public record AssignOwnerRequest(int OwnerId);
public record AssignAssigneesRequest([param: Required] IReadOnlyList<int> AssigneeIds);
public record AssigneeDto(int Id, string Name);

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
    IReadOnlyList<int> PrerequisiteTaskIds,
    IReadOnlyList<AssigneeDto> Assignees,
    bool RequiresReview,
    bool IsApproved,
    DateTime? ApprovedAtUtc);

// ----- Manual Review Gate; SM -----

// Allows a Project Manager to enable or disable review.
public record SetReviewRequirementRequest(bool RequiresReview);

// Allows a Project Manager to approve or reject a task.
public record ReviewTaskRequest(bool Approved);

// ----- Feature 2: status updates -----

public record UpdateStatusRequest(TaskItemStatus Status);

/// <summary>The updated task, plus the ids of any tasks that became Available as a result.</summary>
public record StatusUpdateResult(TaskDto Task, IReadOnlyList<int> UnlockedTaskIds);
