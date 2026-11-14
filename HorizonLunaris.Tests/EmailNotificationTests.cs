using HorizonLunaris.Api.Services;
using HorizonLunaris.Contracts;
using HorizonLunaris.Domain.Entities;
using HorizonLunaris.Domain.Enums;
using HorizonLunaris.Tests.Support;
using Microsoft.Extensions.Options;

namespace HorizonLunaris.Tests;

public class EmailNotificationTests
{
    [Fact]
    public async Task UpdateStatus_ChangedStatus_NotifiesAssignedUsers()
    {
        using var testDb = new TestDb();
        var user = testDb.Context.Users.Single(candidate => candidate.Id == 2);
        var project = new Project { Name = "Project", CreatedById = 1, CreatedAtUtc = DateTime.UtcNow };
        testDb.Context.Projects.Add(project);
        await testDb.Context.SaveChangesAsync();
        var task = new TaskItem
        {
            Title = "Task",
            ProjectId = project.Id,
            Assignees = [user]
        };
        testDb.Context.Tasks.Add(task);
        await testDb.Context.SaveChangesAsync();

        var notifications = new RecordingNotifications();
        var service = new TaskProgressService(
            testDb.Context,
            new FakeCurrentUser(user),
            new ReadyDependencies(),
            notifications);

        await service.UpdateStatusAsync(task.Id, new UpdateStatusRequest(TaskItemStatus.InProgress));
        await service.UpdateStatusAsync(task.Id, new UpdateStatusRequest(TaskItemStatus.InProgress));

        var notification = Assert.Single(notifications.StatusChanges);
        Assert.Equal(TaskItemStatus.Available, notification.PreviousStatus);
        Assert.Equal(TaskItemStatus.InProgress, notification.NewStatus);
        Assert.Equal(user.Id, Assert.Single(notification.Task.Assignees).Id);
    }

    [Fact]
    public async Task UpdateSenderEmail_StoresValidatedAddress()
    {
        using var testDb = new TestDb();
        var manager = testDb.Context.Users.Single(user => user.Role == UserRole.ProjectManager);
        var service = new EmailSenderSettingsService(
            testDb.Context,
            new FakeCurrentUser(manager),
            Options.Create(new EmailSmtpOptions()));

        var result = await service.UpdateAsync("notifications@example.com");

        Assert.Equal("notifications@example.com", result.SourceEmail);
        Assert.False(result.SmtpConfigured);
        Assert.Equal("notifications@example.com", Assert.Single(testDb.Context.EmailNotificationSettings).SourceEmail);
    }

    private sealed class FakeCurrentUser(User user) : ICurrentUser
    {
        public Task<User> GetRequiredAsync(CancellationToken ct = default) => Task.FromResult(user);

        public Task<User> GetRequiredProjectManagerAsync(CancellationToken ct = default) =>
            user.Role is UserRole.ProjectManager or UserRole.Admin
                ? Task.FromResult(user)
                : Task.FromException<User>(new ForbiddenException("Only a Project Manager or Admin can perform this action."));

        public Task<User> GetRequiredAdminAsync(CancellationToken ct = default) =>
            user.Role == UserRole.Admin
                ? Task.FromResult(user)
                : Task.FromException<User>(new ForbiddenException("Only an Admin can manage users."));
    }

    private sealed class ReadyDependencies : IDependencyService
    {
        public Task<IReadOnlyList<TaskDto>> GetPrerequisitesAsync(int taskId, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<TaskDto>>([]);

        public Task<TaskDto> AddDependencyAsync(int taskId, AddDependencyRequest request, CancellationToken ct = default) =>
            throw new NotImplementedException();

        public Task RemoveDependencyAsync(int taskId, int prerequisiteTaskId, CancellationToken ct = default) =>
            throw new NotImplementedException();

        public Task<bool> ArePrerequisitesCompleteAsync(int taskId, CancellationToken ct = default) => Task.FromResult(true);

        public Task<IReadOnlyList<int>> ResolveUnlocksAsync(int completedTaskId, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<int>>([]);
    }

    private sealed class RecordingNotifications : INotificationService
    {
        public List<(TaskItem Task, TaskItemStatus PreviousStatus, TaskItemStatus NewStatus)> StatusChanges { get; } = [];

        public Task NotifyTaskAvailableAsync(TaskItem task, CancellationToken ct = default) => Task.CompletedTask;

        public Task NotifyTaskStatusChangedAsync(
            TaskItem task, TaskItemStatus previousStatus, TaskItemStatus newStatus, CancellationToken ct = default)
        {
            StatusChanges.Add((task, previousStatus, newStatus));
            return Task.CompletedTask;
        }
    }
}
