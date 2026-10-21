using Microsoft.AspNetCore.Mvc;

namespace Horizon_Lunaris_UI.Controllers
{
    public class HomeController : Controller
    {
        // Dashboard (BtnDashboard)
        public IActionResult Index() => View();

        // Target of UseExceptionHandler in Program.cs (non-development only)
        public IActionResult Error() => Content("An unexpected error occurred.");
    }
}
