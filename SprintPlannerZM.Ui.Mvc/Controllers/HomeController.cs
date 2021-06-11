using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SprintPlannerZM.Services.Abstractions;
using SprintPlannerZM.Ui.Mvc.Models;

namespace SprintPlannerZM.Ui.Mvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly IBeheerderService _beheerderService;
        private readonly IExamenroosterService _examenroosterService;
        private readonly IHulpleerlingService _hulpleerlingService;
        private readonly IKlasService _klasService;
        private readonly ILeerkrachtService _leerkrachtService;
        private readonly ILeerlingService _leerlingService;
        private readonly ILeerlingverdelingService _leerlingverdelingService;
        private readonly ILogger<HomeController> _logger;
        private readonly ILokaalService _lokaalService;
        private readonly ISprintlokaalreservatieService _sprintlokaalreservatieService;
        private readonly ISprintvakkeuzeService _sprintvakkeuzeService;
        private readonly IVakService _vakService;

        public HomeController(
            ILogger<HomeController> logger,
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
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Redirecting(string mail)
        {
            Console.WriteLine(mail);
            var mailNaam = mail.Split('@')[0];
            Console.WriteLine(mailNaam);

            //admin check komt hier maar geen admin tabel?
            //if (await _beheerderService.FindMail(mailNaam))
            //    return RedirectToAction("AdminIndex");
            //else
            if (await _beheerderService.FindMail(mailNaam))
            {
                Console.WriteLine("beheerder");
                return RedirectToAction("BeheerderIndex");
            }
            else if (await _leerkrachtService.FindMail(mailNaam))
            {
                Console.WriteLine("leerkracht");
                return RedirectToAction("LeerkrachtIndex");
            }
            else
            {
                var leerling = await _leerlingService.FindMail(mailNaam);
                if (leerling == null)
                {
                    Console.WriteLine("persoon niet in database");
                    return View();
                }
                if (leerling.hulpleerling == null)
                {
                    Console.WriteLine("leerling die niet als hulpleerling is aangeduid");
                    //misschien toast message om naar het secretariaat te gaan.
                    return View();
                }
                else
                {
                    Console.WriteLine("leerling die ook als hulpleerling is aangeduid");
                    return RedirectToAction("LeerlingIndex");
                }
            }
        }

        public IActionResult NietToegestaan()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }

        public IActionResult AdminIndex()
        {
            return RedirectToAction("Index", "Admin", new {area = "AdminArea"});
        }

        public IActionResult BeheerderIndex()
        {
            return RedirectToAction("Index", "Beheerder", new {area = "BeheerderArea"});
        }

        public IActionResult LeerkrachtIndex()
        {
            return RedirectToAction("Index", "Leerkracht", new {area = "LeerkrachtArea"});
        }

        public IActionResult LeerlingIndex()
        {
            return RedirectToAction("Index", "Leerling", new {area = "LeerlingArea"});
        }
    }
}