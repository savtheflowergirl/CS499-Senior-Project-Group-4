using HorizonLunaris.Domain.Entities;
using HorizonLunaris.Domain.Enums;
using HorizonLunaris.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace HorizonLunaris.Tests;

public class DbContextSmokeTests
{
    [Fact]
    public void SeedData_HasOneProjectManagerAndTwoTeamMembers()
    {
        using var db = new TestDb();

        var users = db.Context.Users.ToList();

        Assert.Equal(3, users.Count);
        Assert.Single(users, u => u.Role == UserRole.ProjectManager);
        Assert.Equal(2, users.Count(u => u.Role == UserRole.TeamMember));
    }

    [Fact]
    public void SelfDependency_IsRejectedByTheDatabase()
    {
        using var db = new TestDb();
        var project = new Project { Name = "Test", CreatedById = 1 };
        var task = new TaskItem { Title = "Task", Project = project };
        db.Context.Tasks.Add(task);
        db.Context.SaveChanges();

        db.Context.TaskDependencies.Add(new TaskDependency
        {
            DependentTaskId = task.Id,
            PrerequisiteTaskId = task.Id
        });

        Assert.Throws<DbUpdateException>(() => db.Context.SaveChanges());
    }
}
