using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SprintPlannerZM.Model;
using SprintPlannerZM.Repository;
using SprintPlannerZM.Services.Abstractions;

namespace SprintPlannerZM.Ui.Mvc.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminController : Controller
    {
        public readonly IBeheerderService _beheerderService;
        private readonly IDagdeelService _dagdeelService;
        public readonly IDeadlineService _deadlineService;
        private readonly IExamenroosterService _examenroosterService;
        private readonly IExamentijdspanneService _examentijdspanneService;
        private readonly IHulpleerlingService _hulpleerlingService;
        private readonly IKlasService _klasService;
        private readonly ILeerkrachtService _leerkrachtService;
        private readonly ILeerlingService _leerlingService;
        private readonly ILeerlingverdelingService _leerlingverdelingService;
        private readonly ILokaalService _lokaalService;
        private readonly ISprintlokaalService _sprintlokaalService;
        private readonly ISprintvakService _sprintvakService;
        private readonly IVakService _vakService;

        public AdminController(
            IDeadlineService deadlineService,
            IBeheerderService beheerderService,
            IDagdeelService dagdeelService,
            IExamentijdspanneService examentijdspanneService,
            ILeerlingService leerlingService,
            ILokaalService lokaalService,
            IKlasService klasService,
            ILeerkrachtService leerkrachtService,
            IVakService vakService,
            IExamenroosterService examenroosterService,
            IHulpleerlingService hulpleerlingService,
            ISprintvakService sprintvakService,
            ISprintlokaalService sprintlokaalService,
            ILeerlingverdelingService leerlingverdelingService
        )
        {
            _deadlineService = deadlineService;
            _beheerderService = beheerderService;
            _dagdeelService = dagdeelService;
            _examentijdspanneService = examentijdspanneService;
            _leerlingService = leerlingService;
            _lokaalService = lokaalService;
            _klasService = klasService;
            _leerkrachtService = leerkrachtService;
            _vakService = vakService;
            _examenroosterService = examenroosterService;
            _hulpleerlingService = hulpleerlingService;
            _sprintvakService = sprintvakService;
            _sprintlokaalService = sprintlokaalService;
            _leerlingverdelingService = leerlingverdelingService;
        }

        public IActionResult Index()
        {
            //var vakResult = _beheerderService.Find();
            return View("Index");
        }

        [HttpGet]
        //Alle leerlingen overzicht
        public IActionResult LeerlingenOverzicht()
        {
            var klassen = _klasService.Find();
            foreach (var klas in klassen)
            {
                klas.Leerlingen = _leerlingService.FindByKlasID(klas.klasID);
            }
            return View(klassen);
        }

        [HttpPost]
        public IActionResult LeerlingenOverzicht(Leerling leerling)
        {
            if (leerling.mklas == true || leerling.typer == true || leerling.sprinter == true)
            {
                
                if (existsAsHulpLeerling(leerling.leerlingID))
                { 
                    Console.WriteLine("Bestaat al. niet toegevoegd wel aangepast");
                }
                else
                {
                    Hulpleerling hulpleerling = new Hulpleerling { klasID = leerling.KlasID, leerlingID = leerling.leerlingID, };
                    _hulpleerlingService.Create(hulpleerling);
                }

            }
            _leerlingService.Update(leerling.leerlingID, leerling);
            return RedirectToAction();
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
            var leerkrachten = _leerkrachtService.Find();
            return View(leerkrachten);
        }

        public IActionResult PartialAlleLeerkrachten()
        {
            var leerkrachten = _leerkrachtService.Find();
            return PartialView("PartialAlleLeerkrachten", leerkrachten);
        }

        public IActionResult PartialToezichters()
        {
            var leerkrachten = _leerkrachtService.Find();
            return PartialView("PartialToezichters", leerkrachten);
        }

        public IActionResult PartialGeenToezichters()
        {
            var leerkrachten = _leerkrachtService.Find();
            return PartialView("PartialGeenToezichters", leerkrachten);
        }

        [HttpPost]
        public IActionResult PartialComboToezichters(long leerkrachtID)
        {
            var leerkracht = _leerkrachtService.Get(leerkrachtID);
            return PartialView("PartialComboToezichters", leerkracht);
        }

        public IActionResult Overzichten()
        {
            return View();
        }

        public bool existsAsHulpLeerling(long id)
        {
            Hulpleerling dbHulpLeerling = new Hulpleerling();
            dbHulpLeerling = _hulpleerlingService.GetbyLeerlingId(id);

            if (dbHulpLeerling==null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}