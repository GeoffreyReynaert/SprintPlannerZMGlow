using Microsoft.AspNetCore.Mvc;
using SprintPlannerZM.Model;
using SprintPlannerZM.Services.Abstractions;
using System;
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
    
            return View("Index");
        }

        [HttpGet]
        public async Task<IActionResult> LeerlingenOverzicht()
        {
            var klassen = await _klasService.Find();
            return  View( klassen);
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

        public async Task<IActionResult> AlleLeerlingen()
        {
            var klassen = await _klasService.Find();
            
            return View("LeerlingenOverzicht", klassen);
        }


        public async Task<IActionResult> PartialAlleLeerlingen()
        {
            var klassen = await _klasService.Find();
           
            return PartialView("PartialAlleLeerlingen", klassen);
        }
        [HttpPost]
        public async Task<IActionResult> PartialLeerlingenByKlas(int klasID)
        {
            var leerlingen = await _leerlingService.FindByKlasID(klasID);
            return PartialView("PartialLeerlingenByKlas", leerlingen);
        }


        public async Task<IActionResult> LeerlingVerdeling(string datum)
        {
            var hulpLeerlingen = await _hulpleerlingService.Find();
            var splitPieceDatum = datum.Split(" ")[0];
            var examensPerDatum = await _examenroosterService.FindByDatum(DateTime.ParseExact(splitPieceDatum, "dd/MM/yyyy", null));
            var lokalenVoorSprint = await _lokaalService.FindForSprintAsync();
            foreach (var leerling in hulpLeerlingen)
            {
                foreach (var vak in leerling.Klas.Vakken)
                {
                    foreach (var rooster in examensPerDatum)
                    {
                        if (rooster.vakID == vak.vakID)
                        {
                            Console.WriteLine("Deze leerling :" + leerling.Leerling.voorNaam + " heeft het vak " + vak.vaknaam + "als examen op" + rooster.datum);
                        }
                    }
                }
            }
            foreach (var leerling in hulpLeerlingen)
            {
                foreach (var sprintexam in leerling.Sprintvakken)
                {
                    foreach (var geplandExamen in examensPerDatum)
                    {
                        if (geplandExamen.vakID == sprintexam.vakID)
                        {
                            var lokaal = new Lokaal();
                            Console.WriteLine("Sprintvak keuze ID " + sprintexam.sprintvakID + " voor vak " +
                                                                     sprintexam.Vak.vaknaam + " voor leerling " + leerling.Leerling.voorNaam +" " +leerling.Klas.klasnaam +
                                                                     " staat vast op" + geplandExamen.datum );
                            foreach (var gekozenLokaal in lokalenVoorSprint)
                            {
                                var aantalReservatiesPerlokaal = _sprintlokaalService.FindByExamID(geplandExamen.examenID).Result.Count;
                                if (gekozenLokaal.naamafkorting.Equals("225")&& gekozenLokaal.lokaaltype.Equals("sprint")&& aantalReservatiesPerlokaal <= gekozenLokaal.capaciteit)
                                {
                                    lokaal = gekozenLokaal;
                                    var sprintlokaal = new Sprintlokaal() { tijd = geplandExamen.tijd, datum = geplandExamen.datum, lokaalID = lokaal.lokaalID };
                                    sprintlokaal = await _sprintlokaalService.Create(sprintlokaal);
                                    var leerlingverdeling = new Leerlingverdeling() { hulpleerlingID = leerling.hulpleerlingID, sprintlokaalID = sprintlokaal.sprintlokaalID, examenID = geplandExamen.examenID };
                                    leerlingverdeling = await _leerlingverdelingService.Create(leerlingverdeling);
                                }
                            }
                        }
                    }
                }
            }
            var examenroosters = await _examenroosterService.FindDistinct();
            return View("Klasverdeling", examenroosters);
        }

        [HttpPost]
        public async Task<IActionResult> PartialComboLeerlingen(int leerlingID)
        {
            var leerling = await _leerlingService.Get(leerlingID);
            return PartialView("PartialComboLeerlingen", leerling);
        }

        //Detail alle leerlingen naar leerling uit lijst
        public async Task<IActionResult> LeerlingOverzicht(int leerlingID)
        {
            var leerling = await _leerlingService.Get(leerlingID);
            return PartialView("LeerlingOverzicht", leerling);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateLeerlingen(string leerlingenLijst)
        {
            var leerlingLijst = JArray.Parse(leerlingenLijst);
            var count = 0;
            foreach (var leerling in leerlingLijst)
            {
                count++;
                var student = leerling.ToObject<Leerling>();
                _leerlingService.Update(student.leerlingID, student);
                Console.WriteLine(count);
            }
            return RedirectToAction();
        }

        public async Task<IActionResult> Klasverdeling()
        {
            var examens = await _examenroosterService.FindDistinct();
            return View(examens);
        }

        public async Task<IActionResult> Toezichters()
        {
            var leerkrachten = await _leerkrachtService.Find();
            return View(leerkrachten);
        }

        public async Task<IActionResult> PartialAlleLeerkrachten()
        {
            var leerkrachten = await _leerkrachtService.Find();
            return PartialView("PartialAlleLeerkrachten", leerkrachten);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateToezichter(long leerkrachtID)
        {
            var leerkracht = await _leerkrachtService.Get(leerkrachtID);
            leerkracht.sprintToezichter = leerkracht.sprintToezichter != true;
            _leerkrachtService.Update(leerkracht.leerkrachtID, leerkracht);
            return RedirectToAction();
        }

        public async Task<IActionResult> PartialToezichters()
        {
            var leerkrachten = await _leerkrachtService.Find();
            return PartialView("PartialToezichters", leerkrachten);
        }

        public async Task<IActionResult> PartialGeenToezichters()
        {
            var leerkrachten = await _leerkrachtService.Find();
            return PartialView("PartialGeenToezichters", leerkrachten);
        }

        [HttpPost]
        public async Task<IActionResult> PartialComboToezichters(long leerkrachtID)
        {
            var leerkracht = await _leerkrachtService.Get(leerkrachtID);
            return PartialView("PartialComboToezichters", leerkracht);
        }

        public IActionResult Overzichten()
        {
            return View();
        }

        public bool ExistsAsHulpLeerling(long id)
        {
            var dbHulpLeerling = _hulpleerlingService.GetbyLeerlingId(id);

            return dbHulpLeerling != null;
        }
    }
}