﻿using Microsoft.AspNetCore.Mvc;
using SprintPlannerZM.Model;
using SprintPlannerZM.Services.Abstractions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing.Constraints;
using Newtonsoft.Json.Linq;

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


        public async Task<IActionResult> LeerlingenOverzicht(string sortOrder, string currentFilter, string nameString, string klasString, int? pageNumber)
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
                            var sprintvak = new Sprintvakkeuze
                            { vakID = vak.vakID, sprint = false, typer = false, mklas = false, hulpleerlingID = ietske.hulpleerlingID };
                            await _sprintvakkeuzeService.CreateAsync(sprintvak);
                        }
                    }
                }
                else
                {
                    var dbleerling = await _leerlingService.Get(leerling.leerlingID);
                    if (dbleerling.sprinter || dbleerling.typer || dbleerling.mklas)
                    {
                        Console.WriteLine(dbleerling.voorNaam + "is geen hulpleerling meer.");
                        //deleten van vakken waar dbhulpleerlingid == sprintvakleerlingid
                        //deleten van hulpleerling
                        //overal updaten.
                    }
                }
                await _leerlingService.Update(leerling.leerlingID, leerling);

                Console.WriteLine(leerling.voorNaam + " is toegevoegd.");
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
           !                 Klasverdeling     by Geoff            ! 
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
            var i = 0;
            var j = 0;
            var hulpLeerlingen = await _hulpleerlingService.Find();
            var splitPieceDatum = datum.Split(" ")[0];
            var examensPerDatum = await _examenroosterService.FindByDatum(DateTime.ParseExact(splitPieceDatum, "dd/MM/yyyy", null));
            var lokalenVoorSprint = await _lokaalService.FindForSprintAsync();
            var lokalenVoorTyper = await _lokaalService.FindForTyperAsync();
            var sprintlokaal = new SprintlokaalReservatie();

            //tester om te zien wie van de hulpleerlingen examens heeft op de welbepaalde datum

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

            //effectieve methode 

            foreach (var hulpleerling in hulpLeerlingen)
            {
                foreach (var sprintKeuzeExam in hulpleerling.Sprintvakken)
                {
                    foreach (var geplandExamen in examensPerDatum)
                    {
                        if (geplandExamen.vakID == sprintKeuzeExam.vakID && geplandExamen.groep.Equals("gr1") && (sprintKeuzeExam.sprint || sprintKeuzeExam.typer || sprintKeuzeExam.mklas))
                        {

                            //opmaken van de lokaal reservatie type
                            var sType = "";
                            if (sprintKeuzeExam.mklas)
                            {
                                sType = "mklas";
                            }
                            else if (sprintKeuzeExam.typer)
                            {
                                sType = "typer";
                            }
                            else if (sprintKeuzeExam.sprint)
                            {
                                sType = "sprint";
                            }

                            var lokaalReservaties = _sprintlokaalreservatieService.FindByExamIDAndType(geplandExamen.examenID, sType).Result.Count;
                            var lokaalBeztting = _leerlingverdelingService.FindCapWithExamIDAndType(geplandExamen.examenID, sType).Result.Count;

                            if (lokaalReservaties == 0)
                            {

                                if (lokaalBeztting <= lokalenVoorSprint[i].capaciteit && sType.Equals("sprint"))
                                {
                                    sprintlokaal = new SprintlokaalReservatie() { tijd = geplandExamen.tijd, reservatietype = sType, datum = geplandExamen.datum, lokaalID = lokalenVoorSprint[i].lokaalID, examenID = geplandExamen.examenID };
                                    sprintlokaal = await _sprintlokaalreservatieService.Create(sprintlokaal);
                                    Console.WriteLine("Sprintlokaal reservatie aangemaakt voor type " + sType);
                                    var leerlingverdeling = new Leerlingverdeling() { hulpleerlingID = hulpleerling.hulpleerlingID, sprintlokaalID = sprintlokaal.sprintlokaalreservatieID, examenID = geplandExamen.examenID };
                                    leerlingverdeling = await _leerlingverdelingService.Create(leerlingverdeling);
                                    Console.WriteLine("Sprintvak keuze ID " + sprintKeuzeExam.sprintvakkeuzeID + " voor vak " +
                                                      sprintKeuzeExam.Vak.vaknaam + " voor leerling " + hulpleerling.Leerling.voorNaam + " " + hulpleerling.Klas.klasnaam +
                                                      " staat vast op" + geplandExamen.datum + " als sprinter");

                                }
                                else if (lokaalBeztting <= lokalenVoorTyper[j].capaciteit && sType.Equals("typer"))
                                {
                                    sprintlokaal = new SprintlokaalReservatie() { tijd = geplandExamen.tijd, reservatietype = sType, datum = geplandExamen.datum, lokaalID = lokalenVoorTyper[i].lokaalID, examenID = geplandExamen.examenID };
                                    sprintlokaal = await _sprintlokaalreservatieService.Create(sprintlokaal);
                                    Console.WriteLine("Sprintlokaal reservatie aangemaakt voor type " + sType);
                                    var leerlingverdeling = new Leerlingverdeling() { hulpleerlingID = hulpleerling.hulpleerlingID, sprintlokaalID = sprintlokaal.sprintlokaalreservatieID, examenID = geplandExamen.examenID };
                                    leerlingverdeling = await _leerlingverdelingService.Create(leerlingverdeling);
                                    Console.WriteLine("Sprintvak keuze ID " + sprintKeuzeExam.sprintvakkeuzeID + " voor vak " +
                                                      sprintKeuzeExam.Vak.vaknaam + " voor leerling " + hulpleerling.Leerling.voorNaam + " " + hulpleerling.Klas.klasnaam +
                                                      " staat vast op" + geplandExamen.datum + " als typer");
                                }
                                else if (lokaalBeztting <= lokalenVoorSprint[i].capaciteit && sType.Equals("mklas"))
                                {
                                    sprintlokaal = new SprintlokaalReservatie() { tijd = geplandExamen.tijd, reservatietype = sType, datum = geplandExamen.datum, lokaalID = lokalenVoorTyper[i].lokaalID, examenID = geplandExamen.examenID };
                                    sprintlokaal = await _sprintlokaalreservatieService.Create(sprintlokaal);
                                    Console.WriteLine("Sprintlokaal reservatie aangemaakt voor type " + sType);
                                    var leerlingverdeling = new Leerlingverdeling() { hulpleerlingID = hulpleerling.hulpleerlingID, sprintlokaalID = sprintlokaal.sprintlokaalreservatieID, examenID = geplandExamen.examenID };
                                    leerlingverdeling = await _leerlingverdelingService.Create(leerlingverdeling);
                                    Console.WriteLine("Sprintvak keuze ID " + sprintKeuzeExam.sprintvakkeuzeID + " voor vak " +
                                                      sprintKeuzeExam.Vak.vaknaam + " voor leerling " + hulpleerling.Leerling.voorNaam + " " + hulpleerling.Klas.klasnaam +
                                                      " staat vast op" + geplandExamen.datum + " als mklas");
                                }
                            }
                            else
                            {

                                if (lokaalBeztting <= lokalenVoorSprint[i].capaciteit && sType.Equals("sprint"))
                                {
                                    var leerlingverdeling = new Leerlingverdeling() { hulpleerlingID = hulpleerling.hulpleerlingID, sprintlokaalID = sprintlokaal.sprintlokaalreservatieID, examenID = geplandExamen.examenID };
                                    leerlingverdeling = await _leerlingverdelingService.Create(leerlingverdeling);
                                    Console.WriteLine("Sprintvak keuze ID " + sprintKeuzeExam.sprintvakkeuzeID + " voor vak " +
                                                      sprintKeuzeExam.Vak.vaknaam + " voor leerling " + hulpleerling.Leerling.voorNaam + " " + hulpleerling.Klas.klasnaam +
                                                      " staat vast op" + geplandExamen.datum + " als sprinter");
                                }
                                else if (lokaalBeztting <= lokalenVoorTyper[j].capaciteit && sType.Equals("typer"))
                                {
                                    var leerlingverdeling = new Leerlingverdeling() { hulpleerlingID = hulpleerling.hulpleerlingID, sprintlokaalID = sprintlokaal.sprintlokaalreservatieID, examenID = geplandExamen.examenID };
                                    leerlingverdeling = await _leerlingverdelingService.Create(leerlingverdeling);
                                    Console.WriteLine("Sprintvak keuze ID " + sprintKeuzeExam.sprintvakkeuzeID + " voor vak " +
                                                      sprintKeuzeExam.Vak.vaknaam + " voor leerling " + hulpleerling.Leerling.voorNaam + " " + hulpleerling.Klas.klasnaam +
                                                      " staat vast op" + geplandExamen.datum + " als typer");
                                }
                                else if (lokaalBeztting <= lokalenVoorSprint[i].capaciteit && sType.Equals("mklas"))
                                {
                                    var leerlingverdeling = new Leerlingverdeling() { hulpleerlingID = hulpleerling.hulpleerlingID, sprintlokaalID = sprintlokaal.sprintlokaalreservatieID, examenID = geplandExamen.examenID };
                                    leerlingverdeling = await _leerlingverdelingService.Create(leerlingverdeling);
                                    Console.WriteLine("Sprintvak keuze ID " + sprintKeuzeExam.sprintvakkeuzeID + " voor vak " +
                                                      sprintKeuzeExam.Vak.vaknaam + " voor leerling " + hulpleerling.Leerling.voorNaam + " " + hulpleerling.Klas.klasnaam +
                                                      " staat vast op" + geplandExamen.datum + " als mklas");
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

        public async Task<IActionResult> ConsulterenExamenverdeling()
        {
            var gereserveerdeExamens = await _sprintlokaalreservatieService.Find();
            return View(gereserveerdeExamens);
        }

        //Detail alle leerlingen naar leerling uit lijst
        //public async Task<IActionResult> LeerlingOverzicht(int leerlingID)
        //{
        //    var leerling = _leerlingService.Get(leerlingID);
        //    var klas = _klasService.GetSprintvakWithKlas(leerling.KlasID);
        //    return PartialView("LeerlingOverzicht", klas);
        //    var leerling = await _leerlingService.Get(leerlingID);
        //    return PartialView("LeerlingOverzicht", leerling);
        //}

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

        public bool ExistsAsHulpLeerling(long id)
        {
            var dbHulpLeerling = _hulpleerlingService.GetbyLeerlingId(id);

            return dbHulpLeerling != null;
        }
    }
}