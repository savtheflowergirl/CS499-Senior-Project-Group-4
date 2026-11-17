
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Horizon_Lunaris_UI.Controllers
{
    [Authorize]
    public class AutomationsController : Controller
    {
        // Automations (BtnAutomations)
        public IActionResult Index() => View();
    }
}
