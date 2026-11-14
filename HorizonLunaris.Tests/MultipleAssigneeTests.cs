using HorizonLunaris.Api.Services;
using HorizonLunaris.Contracts;
using HorizonLunaris.Domain.Entities;
using HorizonLunaris.Domain.Enums;
using HorizonLunaris.Tests.Support;

namespace HorizonLunaris.Tests;

public class MultipleAssigneeTests
{
    [Fact]
    public async Task CreateTask_WithMultipleAssignees_ReturnsAllAssignees()
    {
        using var testDb = new TestDb();
        var project = await CreateProjectAsync(testDb);
        var service = new ProjectTaskService(testDb.Context, new FakeCurrentUser(testDb.Context.Users.Single(user => user.Id == 1)));

        var task = await service.CreateTaskAsync(project.Id,
            new CreateTaskRequest("Task", null, null, null, null, [2, 3]));

        Assert.Equal(new[] { 2, 3 }, task.Assignees.Select(assignee => assignee.Id).Order().ToArray());

        var listedTask = Assert.Single(await service.GetTasksAsync(project.Id));
        Assert.Equal(new[] { 2, 3 }, listedTask.Assignees.Select(assignee => assignee.Id).Order().ToArray());
    }

    [Fact]
    public async Task AssignAssignees_WithEmptyList_RemovesAllAssignees()
    {
        using var testDb = new TestDb();
        var project = await CreateProjectAsync(testDb);
        var service = new ProjectTaskService(testDb.Context, new FakeCurrentUser(testDb.Context.Users.Single(user => user.Id == 1)));
        var task = await service.CreateTaskAsync(project.Id,
            new CreateTaskRequest("Task", null, null, null, null, [2, 3]));

        var updatedTask = await service.AssignAssigneesAsync(task.Id, new AssignAssigneesRequest([]));

        Assert.Empty(updatedTask.Assignees);
    }

    private static async Task<Project> CreateProjectAsync(TestDb testDb)
    {
        var project = new Project { Name = "Project", CreatedById = 1, CreatedAtUtc = DateTime.UtcNow };
        testDb.Context.Projects.Add(project);
        await testDb.Context.SaveChangesAsync();
        return project;
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
}
