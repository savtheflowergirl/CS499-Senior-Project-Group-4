
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Horizon_Lunaris_UI.Controllers
{
    [Authorize]
    public class TasksController : Controller
    {
        // Task Management (BtnTaskManagement)
        public IActionResult Index() => View();
    }
}
