namespace HorizonLunaris.Tests;

/// <summary>
/// FEATURE 3 OWNER: these are the cases to cover. Remove Skip as you implement each one.
/// Build the service with: new DependencyService(db.Context, currentUser, notifications)
/// using a fake ICurrentUser (PM or team member) and a fake INotificationService that records calls.
/// </summary>
public class DependencyServiceTests
{
    private const string Todo = "Feature 3: implement with DependencyService";

    [Fact(Skip = Todo)]
    public void AddDependency_AsProjectManager_Succeeds() { }

    [Fact(Skip = Todo)]
    public void AddDependency_AsTeamMember_IsForbidden() { }

    [Fact(Skip = Todo)]
    public void AddDependency_OnSelf_IsRejected() { }

    [Fact(Skip = Todo)]
    public void AddDependency_CreatingACycle_IsRejected() { }

    [Fact(Skip = Todo)]
    public void AddDependency_OnIncompletePrerequisite_BlocksTheDependentTask() { }

    [Fact(Skip = Todo)]
    public void ResolveUnlocks_WithOneOfTwoPrerequisitesDone_StaysBlocked() { }

    [Fact(Skip = Todo)]
    public void ResolveUnlocks_WhenAllPrerequisitesDone_MakesTaskAvailableAndNotifies() { }

    [Fact(Skip = Todo)]
    public void Dependencies_ArePersistedAcrossContexts() { }
}
