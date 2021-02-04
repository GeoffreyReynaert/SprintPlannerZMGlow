using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SprintPlannerZM.Model;
using SprintPlannerZM.Repository;
using SprintPlannerZM.Services.Abstractions;

namespace SprintPlannerZM.Ui.Mvc.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminController: Controller
    {

        private readonly ILeerlingService _leerlingService;
        private readonly IKlasService _klasService;
        private readonly ILeerkrachtService _leerkrachtService;
        private readonly IVakService _vakService;

        public AdminController(
            ILeerlingService leerlingService,
            IKlasService klasService,
            ILeerkrachtService leerkrachtService,
            IVakService vakservice)
        {
            _leerlingService = leerlingService;
            _klasService = klasService;
            _leerkrachtService = leerkrachtService;
            _vakService = vakservice;
        }

        public IActionResult Index()
        {
           
            return View();
        }


        //Alle leerlingen overzicht
        public IActionResult LeerlingenOverzicht()
        {

            var klasResult = _klasService.Find();
            return View(klasResult);
        }

        public IActionResult DropDownKeuze(int id)
        {
            return null;
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
            IList<Leerkracht> leerkrachten = new List<Leerkracht>();
            leerkrachten = _leerkrachtService.Find();
            return View(leerkrachten);
        }

        public IActionResult Overzichten()
        {
            return View();
        }
    }
}
