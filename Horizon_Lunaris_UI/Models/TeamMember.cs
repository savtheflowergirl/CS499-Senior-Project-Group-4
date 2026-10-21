namespace Horizon_Lunaris_UI.Models
{
    public class TeamMember
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int ActiveTasks { get; set; }
    }
}
