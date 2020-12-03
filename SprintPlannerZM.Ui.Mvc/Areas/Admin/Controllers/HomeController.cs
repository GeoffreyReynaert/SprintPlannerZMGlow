using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SprintPlannerZM.Ui.Mvc.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController: Controller
    {


        public IActionResult Index()
        {
            return View();
        }

        public IActionResult LeerlingOverzicht()
        {
            return View();
        }
    }
}
