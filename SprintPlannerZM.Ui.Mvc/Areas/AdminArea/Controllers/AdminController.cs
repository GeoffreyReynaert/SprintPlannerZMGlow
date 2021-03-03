using Microsoft.AspNetCore.Mvc;
using SprintPlannerZM.Model;
using SprintPlannerZM.Services.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            //var klassen = _klasService.Find();
            //foreach (var klas in klassen)
            //{
            //    klas.Leerlingen = _leerlingService.FindByKlasID(klas.klasID);
            //}
            //return View(klassen);
            var leerlingen = _leerlingService.FindAsyncPagingQueryable();
            return View(leerlingen);
        }

        public async Task<IActionResult> Sorting(string sortOrder, string currentFilter, string searchString, int? pageNumber)
        {
            ViewData["KlasSortParm"] = string.IsNullOrEmpty(sortOrder) ? "klasnaam_desc" : "";
            ViewData["VoornaamSortParm"] = sortOrder == "voornaam_desc" ? "voornaam_desc" : "";
            ViewData["FamilienaamSortParm"] = sortOrder == "familienaam_desc" ? "familienaam_desc" : "";
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentSort"] = sortOrder;

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            var leerlingen = from l in _leerlingService.Find()
                select l;
            if (!string.IsNullOrEmpty(searchString))
            {
                leerlingen = leerlingen.Where(s => s.voorNaam.Contains(searchString)
                                               || s.familieNaam.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "klasnaam_desc":
                    leerlingen = leerlingen.OrderByDescending(l => l.Klas.klasnaam);
                    break;
                case "voornaam_desc":
                    leerlingen = leerlingen.OrderBy(l => l.voorNaam);
                    break;
                case "familienaam_desc":
                    leerlingen = leerlingen.OrderByDescending(l => l.familieNaam);
                    break;
                default:
                    leerlingen = leerlingen.OrderBy(l => l.familieNaam);
                    break;
            }
            const int pageSize = 3;
            return View(await PaginatedList<Leerling>.CreateAsync(leerlingen.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        public ActionResult LeerlingAsync(Leerling[] model)
        {
            return View();
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
            var klas = _klasService.GetSprintvakWithKlas(leerling.KlasID);
            return PartialView("LeerlingOverzicht", klas);
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
            var examenroosters = _examenroosterService.findDistinct();
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