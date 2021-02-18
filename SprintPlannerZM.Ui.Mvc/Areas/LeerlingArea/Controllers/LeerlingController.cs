using Microsoft.AspNetCore.Mvc;

namespace SprintPlannerZM.Ui.Mvc.Areas.LeerlingArea.Controllers
{
    [Area("LeerlingArea")]
    public class LeerlingController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
