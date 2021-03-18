using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SprintPlannerZM.Model;
using SprintPlannerZM.Services.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SprintPlannerZM.Ui.Mvc.Areas.AdminArea.Controllers
{
    [Area("AdminArea")]
    public class AdminController : Controller
    {
        public readonly IBeheerderService _beheerderService;
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

        /* PAGING*/
        public async Task<IActionResult> LeerlingenOverzicht(string sortOrder, string currentFilter, string nameString,
            string klasString, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["FamilienaamSortParm"] = string.IsNullOrEmpty(sortOrder) ? "familienaam_asc" : "familienaam_desc";
            ViewData["VoornaamSortParm"] = sortOrder == "voornaam_desc" ? "voornaam_desc" : "";

            if (nameString != null)
            {
                pageNumber = 1;
            }
            else
            {
                nameString = currentFilter;
            }

            ViewData["NameFilter"] = nameString;

            if (klasString != null)
            {
                pageNumber = 1;
            }
            else
            {
                klasString = currentFilter;
            }

            ViewData["KlasFilter"] = klasString;

            var leerlingen = _leerlingService.FindAsyncPagingQueryable();

            if (!string.IsNullOrEmpty(nameString))
            {
                leerlingen = leerlingen.Where(l => l.voorNaam.ToLower().Contains(nameString.ToLower())
                                                   || l.familieNaam.ToLower().Contains(nameString.ToLower()));
            }

            if (!string.IsNullOrEmpty(klasString))
            {
                leerlingen = leerlingen.Where(k => k.Klas.klasnaam.ToLower().Contains(klasString.ToLower()));
            }

            leerlingen = sortOrder switch
            {
                "klasnaam_desc" => leerlingen.OrderByDescending(l => l.Klas.klasnaam),
                "voornaam_desc" => leerlingen.OrderBy(l => l.voorNaam),
                "familienaam_asc" => leerlingen.OrderBy(l => l.familieNaam),
                "familienaam_desc" => leerlingen.OrderByDescending(l => l.familieNaam),
                _ => leerlingen.OrderBy(l => l.voorNaam)
            };
            return View(await PaginatedList<Leerling>.CreateAsync(leerlingen.AsQueryable(), pageNumber ?? 1, 12));
        }

        public async Task<IActionResult> LeerlingUpdate(Leerling[] model)
        {
            foreach (var leerling in model)
            {
                if (leerling.sprinter || leerling.typer || leerling.mklas)
                {
                    if (leerling.hulpleerlingID == null)
                    {
                        var hulpleerling = new Hulpleerling
                        { leerlingID = leerling.leerlingID, klasID = leerling.KlasID };
                        var ietske =
                            await _hulpleerlingService.Create(hulpleerling);
                        leerling.hulpleerlingID = ietske.hulpleerlingID;
                        var leerlingVakken = await _vakService.GetByKlasId(leerling.KlasID);
                        foreach (var vak in leerlingVakken)
                        {
                            var sprintvakkeuze = new Sprintvakkeuze()
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
                    if (dbleerling.sprinter || dbleerling.typer || dbleerling.mklas)
                    {
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
                }
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
                if (exam.tijd.Contains("8"))
                {
                    exams8u.Add(exam);
                }

                if (exam.tijd.Contains("10"))
                {
                    exams10u.Add(exam);
                }

                if (exam.tijd.Contains("13"))
                {
                    exams13u.Add(exam);
                }

                if (exam.tijd.Contains("15"))
                {
                    exams15u.Add(exam);
                }
            }
            //tester om te zien wie van de hulpleerlingen examens heeft op de welbepaalde datum

            foreach (var leerling in hulpLeerlingen)
            {
                foreach (var vak in leerling.Klas.Vakken)
                {
                    foreach (var rooster in examensPerDatum)
                    {
                        if (rooster.vakID == vak.vakID)
                        {
                            Console.WriteLine("Deze leerling :" + leerling.Leerling.voorNaam + " heeft het vak " + vak.vaknaam + "als examen op" + rooster.datum + "om " + rooster.tijd + " Uur");
                        }
                    }
                }
            }

            //effectieve methode 
            if (exams8u.Count < 10 || exams10u.Count < 10 || exams13u.Count < 10 || exams15u.Count < 10)
            {


            }
            else
            {
                await ExamVerdelingPerUur(exams8u, hulpLeerlingen, lokalenVoorSprint, lokalenVoorTyper, lokalenVoorMklas);
                await ExamVerdelingPerUur(exams10u, hulpLeerlingen, lokalenVoorSprint, lokalenVoorTyper, lokalenVoorMklas);
                await ExamVerdelingPerUur(exams13u, hulpLeerlingen, lokalenVoorSprint, lokalenVoorTyper, lokalenVoorMklas);
                await ExamVerdelingPerUur(exams15u, hulpLeerlingen, lokalenVoorSprint, lokalenVoorTyper, lokalenVoorMklas);
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

        public async Task<IActionResult> ConsulterenExamenverdeling()
        {
            var gereserveerdeExamens = await _sprintlokaalreservatieService.Find();
            return View(gereserveerdeExamens);
        }


        public async Task<IActionResult> DetailsExamen(int id)
        {
            var leerlingverdelingen = await _leerlingverdelingService.FindBySprintLokaalReservatie(id);
            return View(leerlingverdelingen);
        }

        //Detail alle leerlingen naar leerling uit lijst
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


        /* !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
           !                                                       !
           !          Updaten van hulp statuus  by niels           !
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

        public IActionResult Overzichten()
        {
            return View();
        }

        public IActionResult AantalPerDul()
        {
            return View();
        }

        public IActionResult ToezichtPerLeerkracht()
        {
            return View();
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
                                ReservatieIndexSprint++;

                                Console.WriteLine("Sprintlokaal reservatie aangemaakt voor type " + sType);
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
                                                  hulpleerling.Leerling.voorNaam + " " + hulpleerling.Klas.klasnaam +
                                                  " staat vast op" + examenPerUur.datum + " " + examenPerUur.tijd +
                                                  " als sprinter");

                            }
                            else if (lokaalBezttingTyper <= lokalenVoorTyper[lokaalIndexTyper].capaciteit &&
                                     sType.Equals("typer") && ReservatieIndexTyper == 0)
                            {
                                typerlokaal = new Sprintlokaalreservatie()
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
                                                  hulpleerling.Leerling.voorNaam + " " + hulpleerling.Klas.klasnaam +
                                                  " staat vast op" + examenPerUur.datum + " " + examenPerUur.tijd + " als typer");
                            }
                            else if (lokaalBezttingMklas <= lokalenVoorSprint[lokaalIndexM].capaciteit &&
                                     sType.Equals("mklas") && ReservatieIndexMklas == 0)
                            {
                                mklaslokaal = new Sprintlokaalreservatie()
                                {
                                    tijd = examenPerUur.tijd,
                                    reservatietype = sType,
                                    datum = examenPerUur.datum,
                                    lokaalID = lokalenVoorMklas[lokaalIndexM].lokaalID,
                                    examenID = examenPerUur.examenID,
                                };
                                mklaslokaal = await _sprintlokaalreservatieService.Create(mklaslokaal);
                                lokaalreservaties.Add(mklaslokaal);
                                Console.WriteLine("Sprintlokaal reservatie aangemaakt voor type " + sType);
                                ReservatieIndexMklas++;

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
                                else if (lokaalIndexTyper < lokalenVoorTyper[lokaalIndexTyper].capaciteit &&
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

                                    typerlokaal = new Sprintlokaalreservatie()
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
                                    ReservatieIndexSprint = 1;
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
                                        lokaalID = lokalenVoorTyper[lokaalIndexM].lokaalID,
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
                                                      " als typer");
                                }
                            }
                        }
                    }
                }
            }

            var biepDBLokaal = await _lokaalService.GetByNameAsync("biep!");

            if (aantalExams< biepDBLokaal.capaciteit && aantalExams > 0 && lokaalreservaties.Count>0)
            {
                var BiepRes = new Sprintlokaalreservatie()
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
                    leerlingverdeling.sprintlokaalreservatieID= BiepRes.sprintlokaalreservatieID;
                    await _leerlingverdelingService.Update(leerlingverdeling.leerlingverdelingID,leerlingverdeling);
                }
                foreach (var lokaalres in lokaalreservaties)
                {
                    await _sprintlokaalreservatieService.Delete(lokaalres.sprintlokaalreservatieID);
                }
            }
        }
    }
}