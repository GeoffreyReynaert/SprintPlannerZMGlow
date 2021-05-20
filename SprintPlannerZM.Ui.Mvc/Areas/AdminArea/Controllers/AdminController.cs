using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SprintPlannerZM.Model;
using SprintPlannerZM.Services.Abstractions;
using SprintPlannerZM.Ui.Mvc.Models;

namespace SprintPlannerZM.Ui.Mvc.Areas.AdminArea.Controllers
{
    [Area("AdminArea")]
    public class AdminController : Controller
    {
        private readonly IBeheerderService _beheerderService;
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

        public AdminController(
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
        }

        public IActionResult Index()
        {
            return View("Index");
        }
        /* !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
           !                                                       !
           !           leerlingenOverzicht      by Geoff           !
           !        Werkend met paging en viewdata om de           !
           !              sortering mogelijk te maken              !
           !                                                       !
           !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/
        public async Task<IActionResult> LeerlingenOverzicht(string sortOrder, string currentFilter, string nameString,
            string klasString, int? pageNumber, bool buttonPress)
        {

            //Sortering uit viewbag om zo de klassering via naam familienaam of klasnaam mogelijk te maken 

            ViewData["CurrentSort"] = sortOrder;
            ViewData["FamilienaamSortParam"] = string.IsNullOrEmpty(sortOrder) ? "familienaam_desc" : "";
            ViewData["VoornaamSortParam"] = sortOrder == "voornaam" ? "voornaam_desc" : "voornaam";
            ViewData["KlasnaamSortParam"] = sortOrder == "klas" ? "klas_desc" : "klas";

            //Viewdata om te filteren op naam of klas volgens de ingave

            if (nameString != null)
            {
                if (buttonPress == false) pageNumber = 1;
            }
            else
            {
                nameString = currentFilter;
            }

            ViewData["NameFilter"] = nameString;

            if (klasString != null)
            {
                if (buttonPress == false) pageNumber = 1;
            }
            else
            {
                klasString = currentFilter;
            }

            ViewData["KlasFilter"] = klasString;

            var leerlingen = _leerlingService.FindAsyncPagingQueryable();

            if (!string.IsNullOrEmpty(nameString))
                leerlingen = leerlingen.Where(l => l.voorNaam.ToLower().Contains(nameString.ToLower())
                                                   || l.familieNaam.ToLower().Contains(nameString.ToLower()));

            if (!string.IsNullOrEmpty(klasString))
                leerlingen = leerlingen.Where(k => k.Klas.klasnaam.ToLower().Contains(klasString.ToLower()));


            // Sortering uit viewbag om zo te kalsseren volgens wat gevraagd word

            switch (sortOrder)
            {
                case "familienaam_desc":
                    leerlingen = leerlingen.OrderByDescending(s => s.familieNaam);
                    break;
                case "klas":
                    leerlingen = leerlingen.OrderBy(s => s.Klas.klasnaam);
                    break;
                case "klas_desc":
                    leerlingen = leerlingen.OrderByDescending(s => s.Klas.klasnaam);
                    break;
                case "voornaam":
                    leerlingen = leerlingen.OrderBy(s => s.voorNaam);
                    break;
                case "voornaam_desc":
                    leerlingen = leerlingen.OrderByDescending(s => s.voorNaam);
                    break;
                default:
                    leerlingen = leerlingen.OrderBy(s => s.familieNaam);
                    break;
            }

            return View(await PaginatedList<Leerling>.CreateAsync(leerlingen.AsQueryable(), pageNumber ?? 1, 12));

        }


        //Detail alle leerlingen naar een specifieke leerling uit lijst
        public async Task<IActionResult> LeerlingOverzicht(long hulpleerlingId)
        {
            var sprintvakken = await _sprintvakkeuzeService.FindWithHll(hulpleerlingId);
            return View("LeerlingOverzicht", sprintvakken);
        }

        public async Task<IActionResult> UpdateVakken(string vakKeuzeLijst)
        {
            var keuzeLijst = JArray.Parse(vakKeuzeLijst);
            var count = 0;
            foreach (var vakKeuze in keuzeLijst)
            {
                count++;
                var sprintvak = vakKeuze.ToObject<Sprintvakkeuze>();
                await _sprintvakkeuzeService.UpdateAsync(sprintvak.sprintvakkeuzeID, sprintvak);
                Console.WriteLine(count);
            }

            return RedirectToAction();
        }
        public async Task<IActionResult> LeerlingUpdate(Leerling[] model)
        {
            foreach (var leerling in model)
                if (leerling.sprinter || leerling.typer || leerling.mklas)
                {
                    if (leerling.hulpleerlingID == null)
                    {
                        var hulpleerling = new Hulpleerling
                            {leerlingID = leerling.leerlingID, klasID = leerling.KlasID};
                        var ietske =
                            await _hulpleerlingService.Create(hulpleerling);
                        leerling.hulpleerlingID = ietske.hulpleerlingID;
                        var leerlingVakken = await _vakService.GetByKlasId(leerling.KlasID);
                        foreach (var vak in leerlingVakken)
                        {
                            var sprintvakkeuze = new Sprintvakkeuze
                            {
                                vakID = vak.vakID,
                                sprint = false,
                                typer = false,
                                mklas = false,
                                hulpleerlingID = ietske.hulpleerlingID
                            };
                            await _sprintvakkeuzeService.CreateAsync(sprintvakkeuze);
                        }
                    }

                    await _leerlingService.Update(leerling.leerlingID, leerling);
                }
                else
                {
                    var dbleerling = await _leerlingService.GetHulpAanpassing(leerling.leerlingID);
                    if (!dbleerling.sprinter && !dbleerling.typer && !dbleerling.mklas) continue;
                    await _sprintvakkeuzeService.DeleteByHulpAsync(dbleerling.hulpleerlingID);
                    var id = dbleerling.hulpleerlingID;
                    dbleerling.hulpleerlingID = null;
                    dbleerling.sprinter = leerling.sprinter;
                    dbleerling.typer = leerling.typer;
                    dbleerling.mklas = leerling.mklas;
                    await _leerlingService.UpdateIdAndType(dbleerling.leerlingID, dbleerling);
                    await _hulpleerlingService.DeleteByAsync(id);
                    Console.WriteLine(dbleerling.voorNaam + " is geen hulpleerling meer.");
                }

            return RedirectToAction("LeerlingenOverzicht");
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


        /* !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
           !                                                       !
           !                 Klasverdeling     by Geoff            !
           ! 
           !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/

        [HttpGet]
        public async Task<IActionResult> Klasverdeling()
        {
            var examens = await _examenroosterService.FindDistinct();

            return View(examens);
        }



        //Verdeling generatie
        public async Task<IActionResult> LeerlingVerdeling(string datum)
        {
            var hulpLeerlingen = await _hulpleerlingService.Find();
            var splitPieceDatum = datum.Split(" ")[0];

            var lokalenVoorSprint = await _lokaalService.FindForSprintAsync();
            var lokalenVoorTyper = await _lokaalService.FindForTyperAsync();
            var lokalenVoorMklas = await _lokaalService.FindForMklasAsync();

            var examensPerDatum =
                await _examenroosterService.FindByDatum(DateTime.ParseExact(splitPieceDatum, "dd/MM/yyyy", null));

            var exams8u = new List<Examenrooster>();
            var exams10u = new List<Examenrooster>();
            var exams13u = new List<Examenrooster>();
            var exams15u = new List<Examenrooster>();

            foreach (var exam in examensPerDatum)
            {
                if (exam.tijd.Contains("8")) exams8u.Add(exam);

                if (exam.tijd.Contains("10")) exams10u.Add(exam);

                if (exam.tijd.Contains("13")) exams13u.Add(exam);

                if (exam.tijd.Contains("15")) exams15u.Add(exam);
            }
            //tester om te zien wie van de hulpleerlingen examens heeft op de welbepaalde datum

            //effectieve methode 

            await ExamVerdelingPerUur(exams8u, hulpLeerlingen, lokalenVoorSprint, lokalenVoorTyper, lokalenVoorMklas);
            await ExamVerdelingPerUur(exams10u, hulpLeerlingen, lokalenVoorSprint, lokalenVoorTyper, lokalenVoorMklas);
            await ExamVerdelingPerUur(exams13u, hulpLeerlingen, lokalenVoorSprint, lokalenVoorTyper, lokalenVoorMklas);
            await ExamVerdelingPerUur(exams15u, hulpLeerlingen, lokalenVoorSprint, lokalenVoorTyper, lokalenVoorMklas);


            var examenroosters = await _examenroosterService.FindDistinct();
            return View("Klasverdeling", examenroosters);
        }

        public async Task<IActionResult> VerdelingVerwijderen(string date)
        {
            var splitPieceDatum = date.Split(" ")[0];
            await _leerlingverdelingService.DeleteAllFromDate(DateTime.ParseExact(splitPieceDatum, "dd/MM/yyyy", null));
            await _sprintlokaalreservatieService.DeleteAllFromDate(DateTime.ParseExact(splitPieceDatum, "dd/MM/yyyy", null));

            var examenroosters = await _examenroosterService.FindDistinct();

            return View("Klasverdeling", examenroosters);
        }



        public async Task<IActionResult> ConsulterenExamenverdeling(string date)
        {
            var splitPieceDatum = date.Split(" ")[0];
            var gereserveerdeExamens = await _sprintlokaalreservatieService.FindByDate(DateTime.ParseExact(splitPieceDatum, "dd/MM/yyyy", null));
            return View(gereserveerdeExamens);
        }

        public async Task<IActionResult> ToezichterToevoegen(int id)
        {
            var lokaalres = await _sprintlokaalreservatieService.Get(id);
            ReservatieModel reservatiemodel = new ReservatieModel
            {
                reservatie = lokaalres,
                Toezichters = await _leerkrachtService.FindToezichters()
            };
            return View(reservatiemodel);
        }

        [HttpPost]
        public async Task<IActionResult> ToezichterToevoegen(ReservatieModel reservatieModel)
        {
      
            var examens = await _examenroosterService.FindDistinct();

            await _sprintlokaalreservatieService.Update(reservatieModel.reservatie.sprintlokaalreservatieID, reservatieModel.reservatie);

            return RedirectToAction("Klasverdeling",examens);
        }


        //Scherm om leerlingen over te zetten naar andere lokalen  

        [HttpGet]
        public async Task<IActionResult> DetailsEnLeerlingWijzigingen(int id)
        {
            var leerlingverdelingen = await _leerlingverdelingService.FindBySprintLokaalReservatie(id);
            var verdelingsLijst =new List<LeerlingVerdelingWijzegingsModel>();
            foreach (var leerlingverdeling in leerlingverdelingen)
            {
                LeerlingVerdelingWijzegingsModel leerlingverdelingsmodel = new LeerlingVerdelingWijzegingsModel
                {
                    leerlingeverdeling = leerlingverdeling,
                    sprintlokaalreservaties = await _sprintlokaalreservatieService.FindByDateTime(leerlingverdeling.Sprintlokaalreservatie.datum,leerlingverdeling.Sprintlokaalreservatie.tijd)
                };
                verdelingsLijst.Add(leerlingverdelingsmodel);
            }
           
            return View(verdelingsLijst);
        }

        [HttpPost]
        public async Task<IActionResult> DetailsEnLeerlingWijzigingen(LeerlingVerdelingWijzegingsModel model)
        {
            var examens = await _examenroosterService.FindDistinct();

         
                await _leerlingverdelingService.Update(model.leerlingeverdeling.leerlingverdelingID, model.leerlingeverdeling);
            
                
            return RedirectToAction("Klasverdeling", examens);
        }


        /* !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
           !                                                       !
           !          Updaten van hulp statuus by niels            !
           ! 
           !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/

        [HttpPost]
        public async Task<IActionResult> UpdateLeerlingen(string leerlingenLijst)
        {
            var leerlingLijst = JArray.Parse(leerlingenLijst);
            var count = 0;
            foreach (var leerling in leerlingLijst)
            {
                count++;
                var student = leerling.ToObject<Leerling>();
                await _leerlingService.Update(student.leerlingID, student);
                Console.WriteLine(count);
            }

            return RedirectToAction();
        }

        [HttpPost]
        public async Task<IActionResult> PartialComboLeerlingen(int leerlingID)
        {
            var leerling = await _leerlingService.Get(leerlingID);
            return PartialView("PartialComboLeerlingen", leerling);
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
            await _leerkrachtService.Update(leerkracht.leerkrachtID, leerkracht);
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

        public async Task<IActionResult> Overzichten()
        {
            var hulpleerlingen = await _hulpleerlingService.simpleFind();

            return View("Overzichten", hulpleerlingen);
        }

        public async Task<IActionResult> AantalPerDul()
        {
            Console.WriteLine("Aantal Per Dul");

            var datums = await _sprintlokaalreservatieService.FindDistinctDatums();

            var listData2 = await _sprintlokaalreservatieService.Find();
            var listData = await _sprintlokaalreservatieService.FindDul();

            //Create excel file.
            var workbook = new XLWorkbook();
            var workSheet = workbook.Worksheets.Add("Aantal Per Dul");
            var paginarow = 1;

            //Test.
            foreach (var datum in datums)
            {
                //Declaratie en bijhouden van de rijen en examen
                var row = paginarow;
                var beginRow = paginarow + 1;
                var endRow = paginarow + 25;
                var examenCount = 0;

                //Datum van examen en styling
                workSheet.Cell($"A{row}").SetValue(datum.DayOfWeek + " " +
                                                   datum.Day + " " +
                                                   datum.ToString("MMMM") + " " +
                                                   datum.Year);
                workSheet.Range($"A{paginarow}", $"D{paginarow}").Merge();
                workSheet.Row(row).Style.Font.SetBold();
                workSheet.Range($"A{row}", $"D{row}").Style.Border.SetBottomBorder(XLBorderStyleValues.Thick);
                workSheet.Range($"A{row}", $"D{row}").Style.Border.SetTopBorder(XLBorderStyleValues.Thick);
                workSheet.Range($"A{row}", $"D{row}").Style.Border.SetLeftBorder(XLBorderStyleValues.Thick);
                workSheet.Range($"A{row}", $"D{row}").Style.Border.SetRightBorder(XLBorderStyleValues.Thick);

                row++;
                var reservaties = await _sprintlokaalreservatieService.GetTime(datum);
                foreach (var reservatie in reservaties)
                {
                    //Header van het uur en styling
                    workSheet.Cell($"A{row}").SetValue(reservatie.datum.Hour + "u" +
                                                       reservatie.datum.Minute);
                    workSheet.Row(row).Style.Font.SetBold();

                    row++;

                    //Header van het lokaal en count van aantal leerlingen en styling
                    workSheet.Cell($"A{row}").SetValue(reservatie.Lokaal.naamafkorting);
                    workSheet.Row(row).Style.Font.SetBold();
                    workSheet.Cell($"B{row}").SetValue(reservatie.Leerlingverdelingen.Count);
                    workSheet.Range($"B{row}", $"C{row}").Merge();

                    row++;

                    foreach (var leerlingenverdeling in reservatie.Leerlingverdelingen)
                    {
                        Console.WriteLine(leerlingenverdeling);

                        workSheet.Cell($"A{row}").SetValue(leerlingenverdeling.reservatietype);
                        workSheet.Cell($"B{row}").SetValue(leerlingenverdeling.Hulpleerling.Klas.klasnaam);
                        workSheet.Cell($"C{row}").SetValue(leerlingenverdeling.Hulpleerling.Leerling.voorNaam + " " +
                                                           leerlingenverdeling.Hulpleerling.Leerling.familieNaam);
                        workSheet.Cell($"D{row}").SetValue(leerlingenverdeling.Examenrooster.Vak.vaknaam);
                        workSheet.Cell($"E{row}").SetValue(leerlingenverdeling.Examenrooster.Vak.Leerkracht.voornaam +
                                                           " " +
                                                           leerlingenverdeling.Examenrooster.Vak.Leerkracht.achternaam +
                                                           " (" +
                                                           leerlingenverdeling.Examenrooster.Vak.Leerkracht.kluisNr +
                                                           ")");
                        row++;
                    }

                    //Markup voor de cellen
                    workSheet.Range($"A{beginRow}", $"A{endRow}").Style.Alignment
                        .SetHorizontal(XLAlignmentHorizontalValues.Center).Alignment
                        .SetVertical(XLAlignmentVerticalValues.Center);
                    workSheet.Range($"B{beginRow}", $"B{endRow}").Style.Alignment
                        .SetHorizontal(XLAlignmentHorizontalValues.Left).Alignment
                        .SetVertical(XLAlignmentVerticalValues.Center);
                    workSheet.Range($"C{beginRow}", $"C{endRow}").Style.Alignment
                        .SetHorizontal(XLAlignmentHorizontalValues.Left).Alignment
                        .SetVertical(XLAlignmentVerticalValues.Center);
                    workSheet.Range($"D{beginRow}", $"D{endRow}").Style.Alignment
                        .SetHorizontal(XLAlignmentHorizontalValues.Left).Alignment
                        .SetVertical(XLAlignmentVerticalValues.Center);
                    workSheet.Range($"E{beginRow}", $"E{endRow}").Style.Alignment
                        .SetHorizontal(XLAlignmentHorizontalValues.Center).Alignment
                        .SetVertical(XLAlignmentVerticalValues.Center);
                    workSheet.Range($"F{beginRow}", $"F{endRow}").Style.Alignment
                        .SetHorizontal(XLAlignmentHorizontalValues.Center).Alignment
                        .SetVertical(XLAlignmentVerticalValues.Center);
                }

                paginarow += 52;
            }

            //Margins verandert om langere namen van leerlingen en leerkrachten te kunnen tonen.
            workSheet.PageSetup.Margins.Top = 0.25;
            workSheet.PageSetup.Margins.Bottom = 0.25;
            workSheet.PageSetup.Margins.Left = 0.25;
            workSheet.PageSetup.Margins.Right = 0.25;
            workSheet.PageSetup.Margins.Footer = 0;
            workSheet.PageSetup.Margins.Header = 0;

            //Max width = 130 voor 1 pagina
            workSheet.Columns("A").Width = 15;
            workSheet.Columns("B").Width = 10;
            workSheet.Columns("C").Width = 20;
            workSheet.Columns("D").Width = 30;
            workSheet.Columns("E").Width = 30;
            workSheet.Rows().Height = 20;

            //Save file
            workbook.SaveAs("AantalPerDagUurLokaal.xlsx");

            return RedirectToAction("Overzichten");
        }

        public async Task<IActionResult> ToezichtPerLeerkracht()
        {
            Console.WriteLine("Toezicht per leerkracht");

            var listData = await _leerkrachtService.FindOverzicht();

            //Create excel file.
            var workbook = new XLWorkbook();
            var workSheet = workbook.Worksheets.Add("Toezicht Per Leerkracht");
            var paginarow = 1;

            //Test.
            foreach (var leerkracht in listData)
            {
                if (leerkracht.Sprintlokaalreservaties.Any())
                {
                    foreach (var sprintlokaalreservaties in leerkracht.Sprintlokaalreservaties)
                    {
                        //Declaratie en bijhouden van de rijen en examen.
                        var row = paginarow;
                        var beginRow = paginarow + 2;
                        var endRow = paginarow + 29;
                        var leerlingCount = 0;

                        //Naam en kluisnummer van leekracht
                        workSheet.Cell($"A{row}").SetValue(leerkracht.voornaam + " " + leerkracht.achternaam + " (" +
                                                           leerkracht.kluisNr + ")");
                        workSheet.Range($"A{paginarow}", $"F{paginarow}").Merge();
                        workSheet.Row(row).Style.Font.SetBold();
                        workSheet.Row(row).Style.Font.SetUnderline();

                        //Headers voor de tabel
                        row += 2;
                        workSheet.Cell($"A{row}").SetValue("Soort");
                        workSheet.Cell($"B{row}").SetValue("Lokaal");
                        workSheet.Cell($"C{row}").SetValue("Klas");
                        workSheet.Cell($"D{row}").SetValue("Naam");
                        workSheet.Cell($"E{row}").SetValue("Proefwerk");
                        workSheet.Cell($"F{row}").SetValue("Vakleerkracht");
                        workSheet.Range($"A{beginRow}", $"F{beginRow}").Style.Border
                            .SetBottomBorder(XLBorderStyleValues.Thin).Border.SetTopBorder(XLBorderStyleValues.Thin);

                        row--;
                        //Datum en uur
                        workSheet.Cell($"A{row}").SetValue(sprintlokaalreservaties.datum.Date.DayOfWeek + " " + 
                                                           sprintlokaalreservaties.datum.Date.Day + " " +
                                                           sprintlokaalreservaties.datum.Date.ToString("MMMM") + " " +
                                                           sprintlokaalreservaties.datum.Date.Year);
                        workSheet.Cell($"D{row}").SetValue(sprintlokaalreservaties.datum.Hour + "u" +
                                                           sprintlokaalreservaties.datum.Minute);

                        //HulpleerlingId van leerlingverdeling waar sprintlokaalreservatie gelijk is aan sprintlokaalreservatieid van die leerkracht
                        var reservatieData =
                            await _sprintlokaalreservatieService.Get(sprintlokaalreservaties.sprintlokaalreservatieID);

                        //Aantal personen
                        workSheet.Cell($"F{row}").SetValue("Aantal: " + sprintlokaalreservaties.Leerlingverdelingen.Count);

                        row += 2;

                        foreach (var leerlingenverdeling in reservatieData.Leerlingverdelingen)
                        {
                            Console.WriteLine(leerlingenverdeling.Hulpleerling.Leerling.voorNaam + " " +
                                              leerlingenverdeling.Hulpleerling.Leerling.familieNaam);

                            //Counter om te controleren hoeveel examens er zijn.
                            leerlingCount += 1;

                            workSheet.Cell($"A{row}").SetValue(leerlingenverdeling.reservatietype);
                            workSheet.Cell($"B{row}")
                                .SetValue(leerlingenverdeling.Sprintlokaalreservatie.Lokaal.naamafkorting);
                            workSheet.Cell($"C{row}").SetValue(leerlingenverdeling.Hulpleerling.Klas.klasnaam);
                            workSheet.Cell($"D{row}").SetValue(leerlingenverdeling.Hulpleerling.Leerling.voorNaam +
                                                               " " +
                                                               leerlingenverdeling.Hulpleerling.Leerling.familieNaam);
                            workSheet.Cell($"E{row}").SetValue(leerlingenverdeling.Examenrooster.Vak.vaknaam);
                            workSheet.Cell($"F{row}").SetValue(
                                leerlingenverdeling.Examenrooster.Vak.Leerkracht.voornaam + " " +
                                leerlingenverdeling.Examenrooster.Vak.Leerkracht.achternaam + " (" + 
                                leerlingenverdeling.Examenrooster.Vak.Leerkracht.kluisNr + ")");
                            row++;
                        }

                        //Markup voor de cellen
                        workSheet.Range($"A{beginRow}", $"A{endRow}").Style.Alignment
                            .SetHorizontal(XLAlignmentHorizontalValues.Center).Alignment
                            .SetVertical(XLAlignmentVerticalValues.Center);
                        workSheet.Range($"B{beginRow}", $"B{endRow}").Style.Alignment
                            .SetHorizontal(XLAlignmentHorizontalValues.Left).Alignment
                            .SetVertical(XLAlignmentVerticalValues.Center);
                        workSheet.Range($"C{beginRow}", $"C{endRow}").Style.Alignment
                            .SetHorizontal(XLAlignmentHorizontalValues.Left).Alignment
                            .SetVertical(XLAlignmentVerticalValues.Center);
                        workSheet.Range($"D{beginRow}", $"D{endRow}").Style.Alignment
                            .SetHorizontal(XLAlignmentHorizontalValues.Left).Alignment
                            .SetVertical(XLAlignmentVerticalValues.Center);
                        workSheet.Range($"E{beginRow}", $"E{endRow}").Style.Alignment
                            .SetHorizontal(XLAlignmentHorizontalValues.Center).Alignment
                            .SetVertical(XLAlignmentVerticalValues.Center);
                        workSheet.Range($"F{beginRow}", $"F{endRow}").Style.Alignment
                            .SetHorizontal(XLAlignmentHorizontalValues.Center).Alignment
                            .SetVertical(XLAlignmentVerticalValues.Center);

                        //Aantal examens controleren om eventueel extra bladen te voorzien.
                        if (leerlingCount > 26)
                        {
                            paginarow += 29;
                            leerlingCount -= 26;
                            var aantal = leerlingCount / 29;
                            for (var i = 0; i < aantal; i++) paginarow += 29;
                        }

                        paginarow += 29;
                    }
                }
            }

            //Margins verandert om langere namen van leerlingen en leerkrachten te kunnen tonen.
            workSheet.PageSetup.Margins.Top = 0.25;
            workSheet.PageSetup.Margins.Bottom = 0.25;
            workSheet.PageSetup.Margins.Left = 0.25;
            workSheet.PageSetup.Margins.Right = 0.25;
            workSheet.PageSetup.Margins.Footer = 0;
            workSheet.PageSetup.Margins.Header = 0;

            //Max width = 130 voor 1 pagina
            workSheet.Columns("A").Width = 15;
            workSheet.Columns("B").Width = 15;
            workSheet.Columns("C").Width = 10;
            workSheet.Columns("D").Width = 34;
            workSheet.Columns("E").Width = 30;
            workSheet.Columns("F").Width = 25;
            workSheet.Rows().Height = 20;


            //Orientation
            workSheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;

            //Save file
            workbook.SaveAs("ToezichtPerLeerkracht.xlsx");

            return RedirectToAction("Overzichten");
        }

        [HttpPost]
        public async Task<IActionResult> ExamenPerLeerling(long hulpleerlingId)
        {

            //Create excel file.
            var workbook = new XLWorkbook();
            var workSheet = workbook.Worksheets.Add("Examen Per Leerling");
            var paginarow = 1;

            if (hulpleerlingId == -1)
            {

                //Oproepen van data.
                var listData = await _hulpleerlingService.FindOverzicht();

                foreach (var hulpleerling in listData)
                    if (hulpleerling.Leerlingverdelingen.Any())
                    {

                        //Declaratie en bijhouden van de rijen en examen.
                        var row = paginarow;
                        var beginRow = paginarow + 1;
                        var endRow = paginarow + 25;
                        var examenCount = 0;

                        //Klas en naam van leerling
                        workSheet.Cell($"A{row}").SetValue(hulpleerling.Klas.klasnaam);
                        workSheet.Cell($"B{row}")
                            .SetValue($"{hulpleerling.Leerling.voorNaam} {hulpleerling.Leerling.familieNaam}");
                        workSheet.Range($"B{paginarow}", $"E{paginarow}").Merge();
                        workSheet.Row(row).Style.Font.SetBold();
                        workSheet.Row(row).Style.Font.SetUnderline();

                        //Headers voor de tabel
                        row++;
                        workSheet.Cell($"A{row}").SetValue("Datum");
                        workSheet.Cell($"B{row}").SetValue("Uur");
                        workSheet.Cell($"C{row}").SetValue("Soort");
                        workSheet.Cell($"D{row}").SetValue("Proefwerk");
                        workSheet.Cell($"E{row}").SetValue("Lokaal");
                        workSheet.Range($"A{beginRow}", $"E{beginRow}").Style.Border
                            .SetBottomBorder(XLBorderStyleValues.Thick);
                        foreach (var leerlingverdeling in hulpleerling.Leerlingverdelingen)
                        {
                            row++;
                            workSheet.Cell($"A{row}").SetValue(leerlingverdeling.Examenrooster.datum.Date);
                            workSheet.Cell($"B{row}").SetValue(leerlingverdeling.Examenrooster.tijd);
                            workSheet.Cell($"C{row}").SetValue(leerlingverdeling.reservatietype);
                            workSheet.Cell($"D{row}").SetValue(leerlingverdeling.Examenrooster.Vak.vaknaam);
                            workSheet.Cell($"E{row}")
                                .SetValue(leerlingverdeling.Sprintlokaalreservatie.Lokaal.naamafkorting);

                            //Counter om te controleren hoeveel examens er zijn.
                            examenCount += 1;
                        }

                        //Markup voor de cellen
                        workSheet.Range($"A{beginRow}", $"A{endRow}").Style.Alignment
                            .SetHorizontal(XLAlignmentHorizontalValues.Center).Alignment
                            .SetVertical(XLAlignmentVerticalValues.Center);
                        workSheet.Range($"B{beginRow}", $"B{endRow}").Style.Alignment
                            .SetHorizontal(XLAlignmentHorizontalValues.Left).Alignment
                            .SetVertical(XLAlignmentVerticalValues.Center);
                        workSheet.Range($"C{beginRow}", $"C{endRow}").Style.Alignment
                            .SetHorizontal(XLAlignmentHorizontalValues.Left).Alignment
                            .SetVertical(XLAlignmentVerticalValues.Center);
                        workSheet.Range($"D{beginRow}", $"D{endRow}").Style.Alignment
                            .SetHorizontal(XLAlignmentHorizontalValues.Left).Alignment
                            .SetVertical(XLAlignmentVerticalValues.Center);
                        workSheet.Range($"E{beginRow}", $"E{endRow}").Style.Alignment
                            .SetHorizontal(XLAlignmentHorizontalValues.Center).Alignment
                            .SetVertical(XLAlignmentVerticalValues.Center);

                        //Aantal examens controleren om eventueel extra bladen te voorzien.
                        if (examenCount > 24)
                        {
                            paginarow += 26;
                            examenCount -= 24;
                            var aantal = examenCount / 26;
                            for (var i = 0; i < aantal; i++) paginarow += 26;
                        }

                        paginarow += 26;
                    }

                //Max width = 110 voor 1 pagina
                workSheet.Columns("A").Width = 25;
                workSheet.Columns("B").Width = 15;
                workSheet.Columns("C").Width = 20;
                workSheet.Columns("D").Width = 40;
                workSheet.Columns("E").Width = 10;
                workSheet.Rows().Height = 20;

                //Orientation
                workSheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;

                //Save file
                workbook.SaveAs("ExamenPerLeerling.xlsx");
            }
            else
            {

                //Oproepen van data.
                var singleData = await _hulpleerlingService.GetOverzicht(hulpleerlingId);

                //Declaratie en bijhouden van de rijen.
                var row = paginarow;
                var beginRow = paginarow + 1;
                var endRow = paginarow + 25;

                //Klas en naam van leerling
                workSheet.Cell($"A{row}").SetValue(singleData.Klas.klasnaam);
                workSheet.Cell($"B{row}")
                    .SetValue($"{singleData.Leerling.voorNaam} {singleData.Leerling.familieNaam}");
                workSheet.Range($"B{paginarow}", $"E{paginarow}").Merge();
                workSheet.Row(row).Style.Font.SetBold();
                workSheet.Row(row).Style.Font.SetUnderline();

                //Headers voor de tabel
                row++;
                workSheet.Cell($"A{row}").SetValue("Datum");
                workSheet.Cell($"B{row}").SetValue("Uur");
                workSheet.Cell($"C{row}").SetValue("Soort");
                workSheet.Cell($"D{row}").SetValue("Proefwerk");
                workSheet.Cell($"E{row}").SetValue("Lokaal");
                workSheet.Range($"A{beginRow}", $"E{beginRow}").Style.Border
                    .SetBottomBorder(XLBorderStyleValues.Thick);
                foreach (var leerlingverdeling in singleData.Leerlingverdelingen)
                {
                    row++;
                    workSheet.Cell($"A{row}").SetValue(leerlingverdeling.Examenrooster.datum.Date);
                    workSheet.Cell($"B{row}").SetValue(leerlingverdeling.Examenrooster.tijd);
                    workSheet.Cell($"C{row}").SetValue(leerlingverdeling.reservatietype);
                    workSheet.Cell($"D{row}").SetValue(leerlingverdeling.Examenrooster.Vak.vaknaam);
                    workSheet.Cell($"E{row}")
                        .SetValue(leerlingverdeling.Sprintlokaalreservatie.Lokaal.naamafkorting);
                }

                //Markup voor de cellen
                workSheet.Range($"A{beginRow}", $"A{endRow}").Style.Alignment
                    .SetHorizontal(XLAlignmentHorizontalValues.Center).Alignment
                    .SetVertical(XLAlignmentVerticalValues.Center);
                workSheet.Range($"B{beginRow}", $"B{endRow}").Style.Alignment
                    .SetHorizontal(XLAlignmentHorizontalValues.Left).Alignment
                    .SetVertical(XLAlignmentVerticalValues.Center);
                workSheet.Range($"C{beginRow}", $"C{endRow}").Style.Alignment
                    .SetHorizontal(XLAlignmentHorizontalValues.Left).Alignment
                    .SetVertical(XLAlignmentVerticalValues.Center);
                workSheet.Range($"D{beginRow}", $"D{endRow}").Style.Alignment
                    .SetHorizontal(XLAlignmentHorizontalValues.Left).Alignment
                    .SetVertical(XLAlignmentVerticalValues.Center);
                workSheet.Range($"E{beginRow}", $"E{endRow}").Style.Alignment
                    .SetHorizontal(XLAlignmentHorizontalValues.Center).Alignment
                    .SetVertical(XLAlignmentVerticalValues.Center);

                //Max width = 110 voor 1 pagina
                workSheet.Columns("A").Width = 25;
                workSheet.Columns("B").Width = 15;
                workSheet.Columns("C").Width = 20;
                workSheet.Columns("D").Width = 40;
                workSheet.Columns("E").Width = 10;
                workSheet.Rows().Height = 20;

                //Orientation
                workSheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;

                //Save file
                workbook.SaveAs("ExamenVoorLeerling.xlsx");
            }

            return RedirectToAction();
        }

        public bool ExistsAsHulpLeerling(long id)
        {
            var dbHulpLeerling = _hulpleerlingService.GetbyLeerlingId(id);

            return dbHulpLeerling != null;
        }


        /* !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
           !                                                       !
           !          Examen verdeling per uur  by Geoff           !
           ! 
           !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/
        public async Task ExamVerdelingPerUur(IList<Examenrooster> examsUurList, IList<Hulpleerling> hulpLeerlingen,
            IList<Lokaal> lokalenVoorSprint, IList<Lokaal> lokalenVoorTyper, IList<Lokaal> lokalenVoorMklas)
        {
            var lokaalIndexI = 0;
            var lokaalIndexTyper = 0;
            var lokaalIndexM = 0;
            var ReservatieIndexSprint = 0;
            var ReservatieIndexTyper = 0;
            var ReservatieIndexMklas = 0;
            var aantalExams = 0;
            var sprintlokaal = new Sprintlokaalreservatie();
            var typerlokaal = new Sprintlokaalreservatie();
            var mklaslokaal = new Sprintlokaalreservatie();
            var leerlingverdelingen = new List<Leerlingverdeling>();
            var lokaalreservaties = new List<Sprintlokaalreservatie>();

            foreach (var examenPerUur in examsUurList)
            {
                foreach (var hulpleerling in hulpLeerlingen)
                {
                    foreach (var sprintVakKeuzeExamen in hulpleerling.Sprintvakkeuzes)
                    {
                        if (examenPerUur.vakID == sprintVakKeuzeExamen.vakID && examenPerUur.groep.Equals("gr1") &&
                            (sprintVakKeuzeExamen.sprint || sprintVakKeuzeExamen.typer || sprintVakKeuzeExamen.mklas))
                        {

                            //opmaken van de lokaal reservatie type
                            var sType = "";
                            if (sprintVakKeuzeExamen.mklas)
                            {
                                sType = "mklas";
                            }
                            else if (sprintVakKeuzeExamen.typer)
                            {
                                sType = "typer";
                            }
                            else if (sprintVakKeuzeExamen.sprint)
                            {
                                sType = "sprint";
                            }

                    //var lokaalReservaties = _sprintlokaalreservatieService.FindAantalBySprintreservatieIdAndType(sprintlokaal.sprintlokaalreservatieID,sType).Result.Count;
                    var lokaalBezttingSprint = _leerlingverdelingService
                        .FindAantalBySprintLokaalId(sprintlokaal.sprintlokaalreservatieID, sType).Result.Count;
                    var lokaalBezttingTyper = _leerlingverdelingService
                        .FindAantalBySprintLokaalId(typerlokaal.sprintlokaalreservatieID, sType).Result.Count;
                    var lokaalBezttingMklas = _leerlingverdelingService
                        .FindAantalBySprintLokaalId(mklaslokaal.sprintlokaalreservatieID, sType).Result.Count;


                    if (lokaalBezttingSprint <= lokalenVoorSprint[lokaalIndexI].capaciteit &&
                        sType.Equals("sprint") && ReservatieIndexSprint == 0)
                    {
                        sprintlokaal = new Sprintlokaalreservatie
                        {
                            tijd = examenPerUur.tijd,
                            reservatietype = sType,
                            datum = examenPerUur.datum,
                            lokaalID = lokalenVoorSprint[lokaalIndexI].lokaalID,
                            examenID = examenPerUur.examenID
                        };
                        sprintlokaal = await _sprintlokaalreservatieService.Create(sprintlokaal);
                        lokaalreservaties.Add(sprintlokaal);
                        ReservatieIndexSprint++;

                        Console.WriteLine("Sprintlokaal reservatie aangemaakt voor type " + sType);
                        var leerlingverdeling = new Leerlingverdeling
                        {
                            hulpleerlingID = hulpleerling.hulpleerlingID,
                            sprintlokaalreservatieID = sprintlokaal.sprintlokaalreservatieID,
                            examenID = examenPerUur.examenID,
                            reservatietype = sType
                        };

                        leerlingverdeling = await _leerlingverdelingService.Create(leerlingverdeling);
                        leerlingverdelingen.Add(leerlingverdeling);
                        aantalExams++;
                        Console.WriteLine("Sprintvak keuze ID " + sprintVakKeuzeExamen.sprintvakkeuzeID +
                                          " voor vak " +
                                          sprintVakKeuzeExamen.Vak.vaknaam + " voor leerling " +
                                          hulpleerling.Leerling.voorNaam + " " + hulpleerling.Klas.klasnaam +
                                          " staat vast op" + examenPerUur.datum + " " + examenPerUur.tijd +
                                          " als sprinter");
                    }
                    else if (lokaalBezttingTyper <= lokalenVoorTyper[lokaalIndexTyper].capaciteit &&
                             sType.Equals("typer") && ReservatieIndexTyper == 0)
                    {
                        typerlokaal = new Sprintlokaalreservatie
                        {
                            tijd = examenPerUur.tijd,
                            reservatietype = sType,
                            datum = examenPerUur.datum,
                            lokaalID = lokalenVoorTyper[lokaalIndexTyper].lokaalID,
                            examenID = examenPerUur.examenID
                        };
                        typerlokaal = await _sprintlokaalreservatieService.Create(typerlokaal);
                        lokaalreservaties.Add(typerlokaal);
                        ReservatieIndexTyper++;

                        Console.WriteLine("Sprintlokaal reservatie aangemaakt voor type " + sType);
                        var leerlingverdeling = new Leerlingverdeling
                        {
                            hulpleerlingID = hulpleerling.hulpleerlingID,
                            sprintlokaalreservatieID = typerlokaal.sprintlokaalreservatieID,
                            examenID = examenPerUur.examenID,
                            reservatietype = sType
                        };

                        leerlingverdeling = await _leerlingverdelingService.Create(leerlingverdeling);
                        leerlingverdelingen.Add(leerlingverdeling);
                        aantalExams++;
                        Console.WriteLine("Sprintvak keuze ID " + sprintVakKeuzeExamen.sprintvakkeuzeID +
                                          " voor vak " +
                                          sprintVakKeuzeExamen.Vak.vaknaam + " voor leerling " +
                                          hulpleerling.Leerling.voorNaam + " " + hulpleerling.Klas.klasnaam +
                                          " staat vast op" + examenPerUur.datum + " " + examenPerUur.tijd +
                                          " als typer");
                    }
                    else if (lokaalBezttingMklas <= lokalenVoorSprint[lokaalIndexM].capaciteit &&
                             sType.Equals("mklas") && ReservatieIndexMklas == 0)
                    {
                        mklaslokaal = new Sprintlokaalreservatie
                        {
                            tijd = examenPerUur.tijd,
                            reservatietype = sType,
                            datum = examenPerUur.datum,
                            lokaalID = lokalenVoorMklas[lokaalIndexM].lokaalID,
                            examenID = examenPerUur.examenID
                        };
                        mklaslokaal = await _sprintlokaalreservatieService.Create(mklaslokaal);
                        lokaalreservaties.Add(mklaslokaal);
                        Console.WriteLine("Sprintlokaal reservatie aangemaakt voor type " + sType);
                        ReservatieIndexMklas++;

                        var leerlingverdeling = new Leerlingverdeling
                        {
                            hulpleerlingID = hulpleerling.hulpleerlingID,
                            sprintlokaalreservatieID = mklaslokaal.sprintlokaalreservatieID,
                            examenID = examenPerUur.examenID,
                            reservatietype = sType
                        };

                                leerlingverdeling = await _leerlingverdelingService.Create(leerlingverdeling);
                                leerlingverdelingen.Add(leerlingverdeling);
                                aantalExams++;
                                Console.WriteLine("Sprintvak keuze ID " + sprintVakKeuzeExamen.sprintvakkeuzeID +
                                                  " voor vak " +
                                                  sprintVakKeuzeExamen.Vak.vaknaam + " voor leerling " +
                                                  hulpleerling.Leerling.voorNaam + " " + hulpleerling.Klas.klasnaam +
                                                  " staat vast op" + examenPerUur.datum + " " + examenPerUur.tijd + " als mklas");
                            }
                            else
                            {

                                //sprintlokaal = _sprintlokaalreservatieService.Get();
                                if (lokaalBezttingSprint < lokalenVoorSprint[lokaalIndexI].capaciteit &&
                                    sType.Equals("sprint") && ReservatieIndexSprint > 0)
                                {
                                    var leerlingverdeling = new Leerlingverdeling()
                                    {
                                        hulpleerlingID = hulpleerling.hulpleerlingID,
                                        sprintlokaalreservatieID = sprintlokaal.sprintlokaalreservatieID,
                                        examenID = examenPerUur.examenID,
                                        reservatietype = sType
                                    };
                                    leerlingverdeling = await _leerlingverdelingService.Create(leerlingverdeling);
                                    leerlingverdelingen.Add(leerlingverdeling);
                                    aantalExams++;
                                    Console.WriteLine("Sprintvak keuze ID " + sprintVakKeuzeExamen.sprintvakkeuzeID +
                                                      " voor vak " +
                                                      sprintVakKeuzeExamen.Vak.vaknaam + " voor leerling " +
                                                      hulpleerling.Leerling.voorNaam + " " +
                                                      hulpleerling.Klas.klasnaam +
                                                      " staat vast op" + examenPerUur.datum + " " + examenPerUur.tijd +
                                                      " als sprinter");
                                }
                                else if (lokaalBezttingTyper < lokalenVoorTyper[lokaalIndexTyper].capaciteit &&
                                         sType.Equals("typer") && ReservatieIndexTyper > 0)
                                {
                                    var leerlingverdeling = new Leerlingverdeling()
                                    {
                                        hulpleerlingID = hulpleerling.hulpleerlingID,
                                        sprintlokaalreservatieID = typerlokaal.sprintlokaalreservatieID,
                                        examenID = examenPerUur.examenID,
                                        reservatietype = sType
                                    };
                                    leerlingverdeling = await _leerlingverdelingService.Create(leerlingverdeling);
                                    leerlingverdelingen.Add(leerlingverdeling);
                                    aantalExams++;
                                    Console.WriteLine("Sprintvak keuze ID " + sprintVakKeuzeExamen.sprintvakkeuzeID +
                                                      " voor vak " +
                                                      sprintVakKeuzeExamen.Vak.vaknaam + " voor leerling " +
                                                      hulpleerling.Leerling.voorNaam + " " +
                                                      hulpleerling.Klas.klasnaam +
                                                      " staat vast op" + examenPerUur.datum + " " + examenPerUur.tijd +
                                                      " als typer");
                                }
                                else if (lokaalBezttingMklas < lokalenVoorMklas[lokaalIndexM].capaciteit &&
                                         sType.Equals("mklas") && ReservatieIndexMklas > 0)
                                {
                                    var leerlingverdeling = new Leerlingverdeling()
                                    {
                                        hulpleerlingID = hulpleerling.hulpleerlingID,
                                        sprintlokaalreservatieID = mklaslokaal.sprintlokaalreservatieID,
                                        examenID = examenPerUur.examenID,
                                        reservatietype = sType
                                    };
                                    leerlingverdeling = await _leerlingverdelingService.Create(leerlingverdeling);
                                    leerlingverdelingen.Add(leerlingverdeling);
                                    aantalExams++;
                                    Console.WriteLine("Sprintvak keuze ID " + sprintVakKeuzeExamen.sprintvakkeuzeID +
                                                      " voor vak " +
                                                      sprintVakKeuzeExamen.Vak.vaknaam + " voor leerling " +
                                                      hulpleerling.Leerling.voorNaam + " " +
                                                      hulpleerling.Klas.klasnaam +
                                                      " staat vast op" + examenPerUur.datum + " " + examenPerUur.tijd +
                                                      " als mklas");
                                }
                                else if (lokaalBezttingSprint >= lokalenVoorSprint[lokaalIndexI].capaciteit &&
                                         sType.Equals("sprint"))
                                {

                                    lokaalIndexI++;
                                    sprintlokaal = new Sprintlokaalreservatie()
                                    {
                                        tijd = examenPerUur.tijd,
                                        reservatietype = sType,
                                        datum = examenPerUur.datum,
                                        lokaalID = lokalenVoorSprint[lokaalIndexI].lokaalID,
                                        examenID = examenPerUur.examenID
                                    };
                                    sprintlokaal = await _sprintlokaalreservatieService.Create(sprintlokaal);
                                    lokaalreservaties.Add(sprintlokaal);
                                    var leerlingverdeling = new Leerlingverdeling()
                                    {
                                        hulpleerlingID = hulpleerling.hulpleerlingID,
                                        sprintlokaalreservatieID = sprintlokaal.sprintlokaalreservatieID,
                                        examenID = examenPerUur.examenID,
                                        reservatietype = sType
                                    };
                                    leerlingverdeling = await _leerlingverdelingService.Create(leerlingverdeling);
                                    leerlingverdelingen.Add(leerlingverdeling);
                                    aantalExams++;
                                    ReservatieIndexSprint = 1;

                            Console.WriteLine("Sprintvak keuze ID " + sprintVakKeuzeExamen.sprintvakkeuzeID +
                                              " voor vak " +
                                              sprintVakKeuzeExamen.Vak.vaknaam + " voor leerling " +
                                              hulpleerling.Leerling.voorNaam + " " +
                                              hulpleerling.Klas.klasnaam +
                                              " staat vast op" + examenPerUur.datum + " " + examenPerUur.tijd +
                                              " als sprinter");
                        }
                        else if (lokaalBezttingTyper >= lokalenVoorTyper[lokaalIndexTyper].capaciteit &&
                                 sType.Equals("typer"))
                        {
                            lokaalIndexTyper++;

                            typerlokaal = new Sprintlokaalreservatie
                            {
                                tijd = examenPerUur.tijd,
                                reservatietype = sType,
                                datum = examenPerUur.datum,
                                lokaalID = lokalenVoorTyper[lokaalIndexTyper].lokaalID,
                                examenID = examenPerUur.examenID
                            };
                            typerlokaal = await _sprintlokaalreservatieService.Create(typerlokaal);
                            lokaalreservaties.Add(typerlokaal);
                            Console.WriteLine("Sprintlokaal reservatie aangemaakt voor type " + sType);

                                    var leerlingverdeling = new Leerlingverdeling()
                                    {
                                        hulpleerlingID = hulpleerling.hulpleerlingID,
                                        sprintlokaalreservatieID = typerlokaal.sprintlokaalreservatieID,
                                        examenID = examenPerUur.examenID,
                                        reservatietype = sType
                                    };
                                    leerlingverdeling = await _leerlingverdelingService.Create(leerlingverdeling);
                                    leerlingverdelingen.Add(leerlingverdeling);
                                    aantalExams++;
                                    ReservatieIndexTyper = 1;
                                    Console.WriteLine("Sprintvak keuze ID " + sprintVakKeuzeExamen.sprintvakkeuzeID +
                                                      " voor vak " +
                                                      sprintVakKeuzeExamen.Vak.vaknaam + " voor leerling " +
                                                      hulpleerling.Leerling.voorNaam + " " +
                                                      hulpleerling.Klas.klasnaam +
                                                      " staat vast op" + examenPerUur.datum + " " + examenPerUur.tijd +
                                                      " als typer");
                                }
                                else if (lokaalBezttingMklas >= lokalenVoorMklas[lokaalIndexM].capaciteit &&
                                         sType.Equals("mklas"))
                                {
                                    lokaalIndexM++;

                                    mklaslokaal = new Sprintlokaalreservatie()
                                    {
                                        tijd = examenPerUur.tijd,
                                        reservatietype = sType,
                                        datum = examenPerUur.datum,
                                        lokaalID = lokalenVoorMklas[lokaalIndexM].lokaalID,
                                        examenID = examenPerUur.examenID
                                    };
                                    mklaslokaal = await _sprintlokaalreservatieService.Create(mklaslokaal);
                                    lokaalreservaties.Add(mklaslokaal);
                                    Console.WriteLine("Sprintlokaal reservatie aangemaakt voor type " + sType);

                                    var leerlingverdeling = new Leerlingverdeling()
                                    {
                                        hulpleerlingID = hulpleerling.hulpleerlingID,
                                        sprintlokaalreservatieID = mklaslokaal.sprintlokaalreservatieID,
                                        examenID = examenPerUur.examenID,
                                        reservatietype = sType
                                    };
                                    leerlingverdeling = await _leerlingverdelingService.Create(leerlingverdeling);
                                    leerlingverdelingen.Add(leerlingverdeling);
                                    aantalExams++;
                                    ReservatieIndexMklas = 1;
                                    Console.WriteLine("Sprintvak keuze ID " + sprintVakKeuzeExamen.sprintvakkeuzeID +
                                                      " voor vak " +
                                                      sprintVakKeuzeExamen.Vak.vaknaam + " voor leerling " +
                                                      hulpleerling.Leerling.voorNaam + " " +
                                                      hulpleerling.Klas.klasnaam +
                                                      " staat vast op" + examenPerUur.datum + " " + examenPerUur.tijd +
                                                      " als mklas");
                                }
                            }
                        }
                    }
                }
            }

            var biepDBLokaal = await _lokaalService.GetByNameAsync("biep!");

            if (aantalExams < biepDBLokaal.capaciteit && aantalExams > 0 && lokaalreservaties.Count > 0)
            {
                var BiepRes = new Sprintlokaalreservatie
                {
                    datum = lokaalreservaties[0].datum,
                    lokaalID = biepDBLokaal.lokaalID,
                    reservatietype = "Mixed",
                    tijd = lokaalreservaties[0].tijd,
                    examenID = lokaalreservaties[0].examenID

                };
                BiepRes = await _sprintlokaalreservatieService.Create(BiepRes);
                foreach (var leerlingverdeling in leerlingverdelingen)
                {
                    leerlingverdeling.sprintlokaalreservatieID = BiepRes.sprintlokaalreservatieID;
                    await _leerlingverdelingService.Update(leerlingverdeling.leerlingverdelingID, leerlingverdeling);
                }

                foreach (var lokaalres in lokaalreservaties)
                    await _sprintlokaalreservatieService.Delete(lokaalres.sprintlokaalreservatieID);
            }
        }
    }
}