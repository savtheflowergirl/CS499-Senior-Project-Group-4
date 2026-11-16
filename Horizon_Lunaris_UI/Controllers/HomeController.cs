
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Horizon_Lunaris_UI.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public IActionResult Index() => View();

        [AllowAnonymous]
        public IActionResult Error()
        {
            return Content("An unexpected error occurred.");
        }
    }
}
