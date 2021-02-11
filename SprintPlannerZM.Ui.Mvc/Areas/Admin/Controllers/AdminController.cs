using Microsoft.AspNetCore.Mvc;
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
    }
}