using Microsoft.AspNetCore.Mvc;

namespace Horizon_Lunaris_UI.Controllers
{
    public class TasksController : Controller
    {
        // Task Management (BtnTaskManagement)
        public IActionResult Index() => View();
    }
}
