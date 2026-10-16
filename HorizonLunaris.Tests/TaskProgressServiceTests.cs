namespace HorizonLunaris.Tests;

/// <summary>FEATURE 2 OWNER: cases to cover. Remove Skip as you implement each one.</summary>
public class TaskProgressServiceTests
{
    private const string Todo = "Feature 2: implement with TaskProgressService";

    [Fact(Skip = Todo)]
    public void UpdateStatus_ToCompleted_SetsCompletedTimestamp() { }

    [Fact(Skip = Todo)]
    public void UpdateStatus_OnBlockedTask_IsRejected() { }

    [Fact(Skip = Todo)]
    public void UpdateStatus_ByUnrelatedTeamMember_IsForbidden() { }

    [Fact(Skip = Todo)]
    public void UpdateStatus_ToCompleted_ReturnsUnlockedTaskIds() { }

    [Fact(Skip = Todo)]
    public void AddComment_UsesCurrentUserAsAuthor() { }
}
