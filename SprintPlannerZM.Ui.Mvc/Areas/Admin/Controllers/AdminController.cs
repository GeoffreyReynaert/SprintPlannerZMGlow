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


        //Alle leerlingen overzicht
        public IActionResult LeerlingenOverzicht()
        {
            return View();
        }


        //Detail alle leerlingen naar leerling uit lijst
        public IActionResult LeerlingOverzicht()
        {
            return View();
        }





        public IActionResult Klasverdeling()
        {
            return View();
        }



        public IActionResult Toezichters()
        {
            return View();
        }
    }
}
