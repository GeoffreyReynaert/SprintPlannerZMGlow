using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SprintPlannerZM.Model;
using SprintPlannerZM.Services.Abstractions;

namespace SprintPlannerZM.Ui.Mvc.Areas.LeerlingArea.Controllers
{
    [Area("LeerlingArea")]
    public class LeerlingController : Controller
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
        private readonly ISprintlokaalreservatieService _sprintlokaalreservatieService;
        private readonly ISprintvakkeuzeService _sprintvakkeuzeService;
        private readonly IVakService _vakService;

        public LeerlingController(
            IDeadlineService deadlineService,
            IBeheerderService beheerderService,
            ILeerlingService leerlingService,
            ILokaalService lokaalService,
            IKlasService klasService,
            ILeerkrachtService leerkrachtService,
            IVakService vakService,
            IExamenroosterService examenroosterService,
            IHulpleerlingService hulpleerlingService,
            ISprintvakkeuzeService sprintvakkeuzeService,
            ISprintlokaalreservatieService sprintlokaalreservatieService,
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
            _sprintvakkeuzeService = sprintvakkeuzeService;
            _sprintlokaalreservatieService = sprintlokaalreservatieService;
            _leerlingverdelingService = leerlingverdelingService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Keuzevak()
        {
            var hulpleerlingen = await _hulpleerlingService.Find();
            return View(hulpleerlingen);
        }

        [HttpPost]
        public async Task<IActionResult> PartialComboLeerlingen(int hulpleerlingID)
        {
            var hulpleerling = await _hulpleerlingService.Get(hulpleerlingID);
            return PartialView("PartialLeerling", hulpleerling);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateLeerlingen(string vakKeuzeLijst, bool anySprint)
        {
            var jvakKeuzeLijst = JArray.Parse(vakKeuzeLijst);
            var count = 0;
            if (anySprint)
            {
                //update
                foreach (var sprintvakKeuze in jvakKeuzeLijst)
                {
                    count++;
                    var keuze = sprintvakKeuze.ToObject<Sprintvakkeuze>();
                    Console.WriteLine(keuze.sprintvakkeuzeID);
                    Console.WriteLine(keuze.sprint);
                    Console.WriteLine(keuze.typer);
                    Console.WriteLine(keuze.mklas);
                    Console.WriteLine(keuze.hulpleerlingID);
                    Console.WriteLine("--------------------------------------------------------------------------------");
                    await _sprintvakkeuzeService.UpdateAsync(keuze.sprintvakkeuzeID, keuze);
                }
            }
            else
            {
                //create
                foreach (var sprintvakKeuze in jvakKeuzeLijst)
                {
                    count++;
                    var keuze = sprintvakKeuze.ToObject<Sprintvakkeuze>();
                    var leerling = await _leerlingService.Get(keuze.hulpleerlingID);
                    var sprintVak = new Sprintvakkeuze()
                        {vakID = keuze.vakID, sprint = keuze.sprint, typer = keuze.typer, mklas = keuze.mklas, hulpleerlingID = (long) leerling.hulpleerlingID};
                    await _sprintvakkeuzeService.CreateAsync(sprintVak);
                }
            }
            return RedirectToAction();
        }
    }
}
