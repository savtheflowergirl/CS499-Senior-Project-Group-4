namespace HorizonLunaris.Tests;

/// <summary>FEATURE 1 OWNER: cases to cover. Remove Skip as you implement each one.</summary>
public class ProjectTaskServiceTests
{
    private const string Todo = "Feature 1: implement with ProjectTaskService";

    [Fact(Skip = Todo)]
    public void CreateProject_AsProjectManager_Succeeds() { }

    [Fact(Skip = Todo)]
    public void CreateProject_AsTeamMember_IsForbidden() { }

    [Fact(Skip = Todo)]
    public void CreateTask_WithParentInAnotherProject_IsRejected() { }

    [Fact(Skip = Todo)]
    public void CreateSubTask_LinksToParent() { }

    [Fact(Skip = Todo)]
    public void AssignOwner_WithUnknownUser_IsNotFound() { }
}
