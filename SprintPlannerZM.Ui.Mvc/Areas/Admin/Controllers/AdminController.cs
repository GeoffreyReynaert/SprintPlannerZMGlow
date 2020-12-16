using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SprintPlannerZM.Ui.Mvc.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminController: Controller
    {


        public IActionResult Index()
        {
            return View();
        }

        public IActionResult LeerlingOverzicht()
        {
            return View();
        }

        public IActionResult Klasverdeling()
        {
            return View();
        }

        public IActionResult Overzichten()
        {
            return View();
        }

        public IActionResult Toezichters()
        {
            return View();
        }
    }
}
