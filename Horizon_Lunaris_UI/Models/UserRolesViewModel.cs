namespace Horizon_Lunaris_UI.Models
{
    public class UserRolesViewModel
    {
        public string? Search { get; set; }
        public List<TeamMember> Members { get; set; } = new();
    }
}
