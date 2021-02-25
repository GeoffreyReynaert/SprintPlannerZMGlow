using Microsoft.AspNetCore.Mvc;
using SprintPlannerZM.Model;
using SprintPlannerZM.Services.Abstractions;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Newtonsoft.Json.Linq;

namespace SprintPlannerZM.Ui.Mvc.Areas.AdminArea.Controllers
{
    [Area("AdminArea")]
    public class AdminController : Controller
    {
        public readonly IBeheerderService _beheerderService;
        public readonly IDeadlineService _deadlineService;
        private readonly IExamenroosterService _examenroosterService;
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
                if (ExistsAsHulpLeerling(leerling.leerlingID))
                {
                    Console.WriteLine("Bestaat al. niet toegevoegd wel aangepast");
                }
                else
                {
                    Hulpleerling hulpleerling = new Hulpleerling { klasID = leerling.KlasID, leerlingID = leerling.leerlingID };
                    _hulpleerlingService.Create(hulpleerling);
                }
            }
            _leerlingService.Update(leerling.leerlingID, leerling);
            return RedirectToAction();
        }

        public IActionResult AlleLeerlingen()
        {
            var klassen = _klasService.Find();
            foreach (var klas in klassen)
            {
                klas.Leerlingen = _leerlingService.FindByKlasID(klas.klasID);
            }
            return View("LeerlingenOverzicht", klassen);
        }
        public IActionResult PartialAlleLeerlingen()
        {
            var klassen = _klasService.Find();
            foreach (var klas in klassen)
            {
                klas.Leerlingen = _leerlingService.FindByKlasID(klas.klasID);
            }
            return PartialView("PartialAlleLeerlingen", klassen);
        }
        [HttpPost]
        public IActionResult PartialLeerlingenByKlas(int klasID)
        {
            var leerlingen = _leerlingService.FindByKlasID(klasID);
            return PartialView("PartialLeerlingenByKlas", leerlingen);
        }

        public IActionResult LeerlingVerdeling(string datum)
        {

            IList<>
            var hulpLeerlingen = _hulpleerlingService.Find();
            var splitPieceDatum = datum.Split(" ")[0];
            var examensPerDatum = _examenroosterService.FindByDatum(DateTime.ParseExact(splitPieceDatum, "dd/MM/yyyy", null));

            foreach (var leerling in hulpLeerlingen)
            {
                foreach (var vak in leerling.Klas.Vakken)
                {
                    foreach (var rooster in examensPerDatum)
                    {
                        if (rooster.vakID==vak.vakID)
                        {
                            Console.WriteLine("Deze leerling :"+ leerling.Leerling.voorNaam + " heeft het vak "+vak.vaknaam + "als examen op" + rooster.datum  );
                        }
                    }
                }
            }
            var examenroosters = _examenroosterService.FindDistinct();
            return View("Klasverdeling", examenroosters);
        }

        [HttpPost]
        public IActionResult PartialComboLeerlingen(int leerlingID)
        {
            var leerling = _leerlingService.Get(leerlingID);
            return PartialView("PartialComboLeerlingen", leerling);
        }

        //Detail alle leerlingen naar leerling uit lijst
        public IActionResult LeerlingOverzicht(int leerlingID)
        {
            var leerling = _leerlingService.Get(leerlingID);
            return PartialView("LeerlingOverzicht", leerling);
        }

        [HttpPost]
        public IActionResult UpdateLeerlingen(string leerlingenLijst)
        {
            var leerlingLijst = JArray.Parse(leerlingenLijst);
            var count = 0;
            foreach (var leerling in leerlingLijst)
            {
                count++;
                var student = leerling.ToObject<Leerling>();
                if (student.mklas || student.typer || student.sprinter)
                {
                    if (ExistsAsHulpLeerling(student.leerlingID))
                    {
                        Console.WriteLine("Bestaat al. niet toegevoegd wel aangepast");
                    }
                    else
                    {
                        Hulpleerling hulpleerling = new Hulpleerling { klasID = student.KlasID, leerlingID = student.leerlingID };
                        _hulpleerlingService.Create(hulpleerling);
                    }
                }
                _leerlingService.Update(student.leerlingID, student);
                Console.WriteLine(count);
            }
            return RedirectToAction();
        }

        public IActionResult Klasverdeling()
        {
            var examenroosters = _examenroosterService.FindDistinct();
            return View(examenroosters);
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

        [HttpPost]
        public IActionResult UpdateToezichter(long leerkrachtID)
        {
            var leerkracht = _leerkrachtService.Get(leerkrachtID);
            leerkracht.sprintToezichter = leerkracht.sprintToezichter != true;
            _leerkrachtService.Update(leerkracht.leerkrachtID, leerkracht);
            return RedirectToAction();
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

        public bool ExistsAsHulpLeerling(long id)
        {
            Hulpleerling dbHulpLeerling = new Hulpleerling();
            dbHulpLeerling = _hulpleerlingService.GetbyLeerlingId(id);

            return dbHulpLeerling != null;
        }
    }
}