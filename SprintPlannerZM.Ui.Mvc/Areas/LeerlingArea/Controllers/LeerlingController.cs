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
        private readonly ISprintlokaalService _sprintlokaalService;
        private readonly ISprintvakService _sprintvakService;
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
            return View();
        }

        public async Task<IActionResult> Keuzevak()
        {
            var leerlingen = await _leerlingService.Find();
            return View(leerlingen);
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
                    var keuze = sprintvakKeuze.ToObject<Sprintvak>();
                    Console.WriteLine(keuze.sprintvakID);
                    Console.WriteLine(keuze.sprint);
                    Console.WriteLine(keuze.typer);
                    Console.WriteLine(keuze.mklas);
                    Console.WriteLine(keuze.hulpleerlingID);
                    Console.WriteLine("--------------------------------------------------------------------------------");
                    //if (keuze.sprint || keuze.typer || keuze.mklas)
                    //{
                    await _sprintvakService.UpdateAsync(keuze.sprintvakID, keuze);
                    //}
                }
            }
            else
            {
                //create
                foreach (var sprintvakKeuze in jvakKeuzeLijst)
                {
                    count++;
                    var keuze = sprintvakKeuze.ToObject<Sprintvak>();
                    var leerling = await _leerlingService.Get(keuze.hulpleerlingID);
                    var sprintVak = new Sprintvak()
                        {vakID = keuze.vakID, sprint = keuze.sprint, typer = keuze.typer, mklas = keuze.mklas, hulpleerlingID = (long) leerling.hulpleerlingID};
                    await _sprintvakService.CreateAsync(sprintVak);
                }
            }
            return RedirectToAction();
        }
    }
}
