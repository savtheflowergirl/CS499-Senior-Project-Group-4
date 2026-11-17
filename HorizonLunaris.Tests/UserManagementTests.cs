using HorizonLunaris.Api.Services;
using HorizonLunaris.Contracts;
using HorizonLunaris.Domain.Entities;
using HorizonLunaris.Domain.Enums;
using HorizonLunaris.Tests.Support;

namespace HorizonLunaris.Tests;

public class UserManagementTests
{
    [Fact]
    public async Task Admin_CanCreateUserAndUpdateEmail()
    {
        using var testDb = new TestDb();
        var admin = testDb.Context.Users.Single(user => user.Role == UserRole.Admin);
        var service = new UserManagementService(testDb.Context, new FakeCurrentUser(admin));

        var created = await service.CreateAsync(
            new CreateUserRequest(" New Teammate ", "New.User@example.com", UserRole.TeamMember));
        var updated = await service.UpdateEmailAsync(created.Id, new UpdateUserEmailRequest("new.address@example.com"));

        Assert.Equal("New Teammate", created.Name);
        Assert.Equal("new.user@example.com", created.Email);
        Assert.Equal(UserRole.TeamMember, created.Role);
        Assert.Equal("new.address@example.com", updated.Email);
    }

    [Fact]
    public async Task ProjectManager_CannotCreateUsers()
    {
        using var testDb = new TestDb();
        var manager = testDb.Context.Users.Single(user => user.Role == UserRole.ProjectManager);
        var service = new UserManagementService(testDb.Context, new FakeCurrentUser(manager));

        await Assert.ThrowsAsync<ForbiddenException>(() => service.CreateAsync(
            new CreateUserRequest("New Teammate", "new.user@example.com", UserRole.TeamMember)));
    }

    [Fact]
    public async Task Admin_IsAuthorizedForProjectManagerOperations()
    {
        using var testDb = new TestDb();
        var admin = testDb.Context.Users.Single(user => user.Role == UserRole.Admin);
        var currentUser = new FakeCurrentUser(admin);

        var authorized = await currentUser.GetRequiredProjectManagerAsync();

        Assert.Equal(admin.Id, authorized.Id);
    }

    [Fact]
    public async Task CreateUser_RejectsDuplicateEmailCaseInsensitively()
    {
        using var testDb = new TestDb();
        var admin = testDb.Context.Users.Single(user => user.Role == UserRole.Admin);
        var service = new UserManagementService(testDb.Context, new FakeCurrentUser(admin));

        await Assert.ThrowsAsync<BusinessRuleException>(() => service.CreateAsync(
            new CreateUserRequest("Duplicate", "PM@EXAMPLE.COM", UserRole.TeamMember)));
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
