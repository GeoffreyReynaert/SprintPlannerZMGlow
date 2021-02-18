using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SprintPlannerZM.Ui.Mvc.Areas.LeerkrachtArea.Controllers
{
    public class LeerkrachtController : Controller
    {
        [Area("LeerkrachtArea")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
