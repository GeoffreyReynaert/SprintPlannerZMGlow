using System;
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

        public IActionResult Keuzevak()
        {
            var leerlingen = _leerlingService.Find();
            return View(leerlingen);
        }

        [HttpPost]
        public IActionResult PartialComboLeerlingen(int leerlingID)
        {
            var leerling = _leerlingService.Get(leerlingID);
            leerling.Klas = _klasService.GetSprintvakWithKlas(leerling.KlasID);
            return PartialView("PartialLeerling", leerling);
        }

        //[HttpPost]
        //public IActionResult UpdateLeerlingen(string vakKeuzeLijst)
        //{
        //    var JvakKeuzeLijst = JArray.Parse(vakKeuzeLijst);
        //    var count = 0;
        //    foreach (var leerling in JvakKeuzeLijst)
        //    {
        //        count++;
        //        var student = leerling.ToObject<Leerling>();
        //        if (student.mklas || student.typer || student.sprinter)
        //        {
        //            if (ExistAsKeuzeVak(student.leerlingID))
        //            {
        //                Console.WriteLine("Bestaat al. niet toegevoegd wel aangepast");
        //            }
        //            else
        //            {
        //                Hulpleerling hulpleerling = new Hulpleerling { klasID = student.KlasID, leerlingID = student.leerlingID };
        //                _hulpleerlingService.Create(hulpleerling);
        //            }
        //        }
        //        _leerlingService.Update(student.leerlingID, student);
        //        Console.WriteLine(count);
        //    }
        //    return RedirectToAction();
        //}

        public bool ExistAsKeuzeVak(int id)
        {
            var KeuzeVak = new Sprintvak();
            KeuzeVak = _sprintvakService.Get(id);

            return KeuzeVak != null;
        }
    }
}
