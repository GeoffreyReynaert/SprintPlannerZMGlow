using System;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SprintPlannerZM.Services.Abstractions;

namespace SprintPlannerZM.Ui.Mvc.Areas.LeerkrachtArea.Controllers
{
    [Area("LeerkrachtArea")]
    public class LeerkrachtController : Controller
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

        public LeerkrachtController(
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

        public async Task<IActionResult> Uploadzone()
        {
            var leerkrachten = await _leerkrachtService.FindBasic();
            return View(leerkrachten);
        }

        [HttpPost]
        public async Task<IActionResult> LeerkrachtUpload(long leerkrachtId)
        {
            var leerkracht = await _leerkrachtService.Get(leerkrachtId);
            return PartialView("LeerkrachtUpload", leerkracht);
        }

        [HttpPost]
        public async Task<IActionResult> PartialAlleVakken(long leerkrachtId)
        {
            var examenvakken = await _examenroosterService.GetByLeerkracht(leerkrachtId);
            Console.WriteLine(examenvakken.ToString());
            return PartialView("PartialAlleVakken", examenvakken);
        }

        [HttpPost]
        public async Task<IActionResult> PartialNietIngediend(long leerkrachtId)
        {
            var leerkracht = await _leerkrachtService.Get(leerkrachtId);
            return PartialView("PartialNietIngediend", leerkracht);
        }

        [HttpPost]
        public async Task<IActionResult> PartialIngediend(long leerkrachtId)
        {
            var leerkracht = await _leerkrachtService.Get(leerkrachtId);
            return PartialView("PartialIngediend", leerkracht);
        }

        //[HttpPost]
        //public async Task<IActionResult> Indienen(IFormFile pdfSend)
        //{
        //    var examenrooster = await _examenroosterService.Get(7373);
        //    Console.WriteLine(pdfSend);
        //    if (pdfSend.Length <= 0) return RedirectToAction("Uploadzone");
        //    await using (var target = new MemoryStream())
        //    {
        //        await pdfSend.CopyToAsync(target);
        //        examenrooster.examendoc = target.ToArray();
        //    }

        //    await _examenroosterService.UpdateDocument(7373, examenrooster);
        //    return RedirectToAction("Uploadzone");
        //}

        public async Task<IActionResult> Download()
        {
            var examenrooster = await _examenroosterService.Get(7373);
            var pdfDownload = examenrooster.examendoc;
            return File(pdfDownload, MediaTypeNames.Application.Octet, "7373.pdf");
        }

        [HttpPost]
        public async Task<IActionResult> Indienen(string pdfSend, long[] examenId)
        {
            var examenrooster = await _examenroosterService.Get(7373);
            Console.WriteLine(pdfSend);
            //if (pdfSend.Length <= 0) return RedirectToAction("Uploadzone");
            //await using (var target = new MemoryStream())
            //{
            //    await pdfSend.CopyToAsync(target);
            //    examenrooster.examendoc = target.ToArray();
            //}

            await _examenroosterService.UpdateDocument(7373, examenrooster);
            return RedirectToAction("Uploadzone");
        }
    }
}
