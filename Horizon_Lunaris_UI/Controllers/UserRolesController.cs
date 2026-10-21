using Horizon_Lunaris_UI.Models;
using Microsoft.AspNetCore.Mvc;

namespace Horizon_Lunaris_UI.Controllers
{
    public class UserRolesController : Controller
    {
        // Placeholder data until the real database / Account Management component is wired in.
        private static readonly List<TeamMember> SampleMembers = new()
        {
            new() { Name = "Savannah", Email = "savannah@example.com", Role = "Project Manager", ActiveTasks = 4 },
            new() { Name = "Ellie",    Email = "ellie@example.com",    Role = "Developer",       ActiveTasks = 3 },
            new() { Name = "Quan",     Email = "quan@example.com",     Role = "Developer",       ActiveTasks = 5 },
            new() { Name = "Daniel",   Email = "daniel@example.com",   Role = "Developer",       ActiveTasks = 2 },
        };

        // GET /UserRoles?search=ell
        public IActionResult Index(string? search)
        {
            var members = SampleMembers.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                members = members.Where(m =>
                    m.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    m.Email.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    m.Role.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            return View(new UserRolesViewModel
            {
                Search = search,
                Members = members.ToList()
            });
        }
    }
}
