using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SmartSchoolSoapApi;
using SprintPlannerZM.Model;
using SprintPlannerZM.Services.Abstractions;
using SprintPlannerZM.Ui.Mvc.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ExcelDataReader;
using Microsoft.AspNetCore.Http;

// ReSharper disable CommentTypo

namespace SprintPlannerZM.Ui.Mvc.Areas.BeheerderArea.Controllers
{
    [Area("BeheerderArea")]
    public class BeheerderController : Controller
    {

        private readonly AppSettings _appSettings;
        private readonly ILeerkrachtService _leerkrachtService;
        private readonly IKlasService _klasService;
        private readonly ILeerlingService _leerlingService;
        private readonly IVakService _vakService;
        private readonly ILokaalService _lokaalService;
        private readonly IExamenroosterService _examenroosterService;


        public BeheerderController(AppSettings appSettings, ILeerkrachtService leerkrachtService,
            IKlasService klasService, ILeerlingService leerlingService, IVakService vakService, ILokaalService lokaalService, IExamenroosterService examenroosterService)
        {
            _appSettings = appSettings;
            _leerkrachtService = leerkrachtService;
            _klasService = klasService;
            _leerlingService = leerlingService;
            _vakService = vakService;
            _lokaalService = lokaalService;
            _examenroosterService = examenroosterService;
        }


        /*!!!!!!!!!  Navigatie  !!!!!!!!!!!*/

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ImportPagina()
        {
            return View();
        }


        /*!!!!!!!!!!!!  Importeren van start gegevens   !!!!!!!!!!!
          !               Titularissen en klassen                 ! 
          !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!  Connectie met Soap Api
          !        klassen, Studenten, leerkrachten, vakken       !
          !           en de relatie die deze verbind              !  Berichten weergave via partial en ajax call
          !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/

        [HttpGet]
        public async Task<IActionResult> ImportKlasTitularisEnKlas()
        {
            var index = 1;
            IList<TitularisEnKlasSoap> titularisenMetKlas = new List<TitularisEnKlasSoap>();
            IList<string> geschrevenResults = new List<string>();

            var SoapSSApi = SoapConnection();
            var result = await SoapSSApi.getClassTeachersAsync(_appSettings.SsApiPassword, true);
            JArray users = JArray.Parse(result.ToString());

            foreach (var user in users)
            {
                var titularisMetKlas = user.ToObject<TitularisEnKlasSoap>();
                if (titularisMetKlas.stamboeknummer == "NULL")
                {
                    titularisMetKlas.stamboeknummer = index.ToString();
                    index++;
                }
                titularisenMetKlas.Add(titularisMetKlas);
            }

            //opvangen soap fouten
            var leerkacht = new Leerkracht() { leerkrachtID = 1, voornaam = "isEen", achternaam = "foutOpvanger", email = "test.test", status = false };
            var klas = new Klas() { klasID = 1, klasnaam = "0", titularisID = 1 };
            await _leerkrachtService.Create(leerkacht);
            await _klasService.CreateAsync(klas);

            var i = 1;
            foreach (var soapLeerkrachtEnKlas in titularisenMetKlas)
            {
                Leerkracht titularis = SoapNaarleerkrachtmaker(soapLeerkrachtEnKlas);
                Klas klasMetTitul = KlasMaker(soapLeerkrachtEnKlas);

                await _leerkrachtService.Create(titularis);
                await _klasService.CreateAsync(klasMetTitul);
                geschrevenResults.Add("Klas " + i + "/" + titularisenMetKlas.Count + " met ID " + klasMetTitul.klasID + " klasnaam " + klasMetTitul.klasnaam + "/r/n"
                                      + " met titularisID " + klasMetTitul.titularisID + " naam " + titularis.achternaam + " " + titularis.voornaam);
                i++;
            }
            return PartialView("PartialBerichtenResults", geschrevenResults);
        }

        [HttpGet]
        public async Task<IActionResult> ImportStudentklasLeerkrachtVak()
        {
            IList<string> geschrevenMessages = new List<string>();
            IList<Leerkracht> vakleerkrachten = new List<Leerkracht>();
            IList<Leerling> leerlingen = new List<Leerling>();
            IList<Vak> vakken = new List<Vak>();
            IList<long> stamboekenList = new List<long>();
            var idMaker = 1;

            var SoapSSApi = SoapConnection();
            var result = await SoapSSApi.getSkoreClassTeacherCourseRelationAsync(
                _appSettings.SsApiPassword);

            XDocument xDoc = XDocument.Parse(result.ToString());

            /*!!!     Opmaken van alle leerlingen per vak omdat Getalle leerlingen in SS niet bestaat.
                           deze lijst dan distinct gemaakt en foreach toegevoegd aan de database     !!!*/

            foreach (XElement element2 in xDoc.Descendants("leerling"))
            {
                //lln zonder intern nr zijn verlaters
                if (element2.Element("internnummer").Value != "")
                {
                    Console.WriteLine(element2.Parent.Parent.Element("klasnaam").Value);
                    var dbKlas = await _klasService.GetByKlasName(element2.Parent.Parent.Element("klasnaam").Value);

                    var soapLeerling = new Leerling
                    {
                        leerlingID = long.Parse(element2.Element("stamboeknummer").Value),
                        familieNaam = element2.Element("naam").Value,
                        voorNaam = element2.Element("voornaam").Value,
                        email = element2.Element("gebruikersnaam").Value,
                        KlasID = dbKlas.klasID
                    };
                    leerlingen.Add(soapLeerling);
                }
            }

            var distinctLeerlingen = leerlingen.Distinct().ToList();

            var i = 1;
            foreach (var leerling in distinctLeerlingen)
            {
                geschrevenMessages.Add(leerling.voorNaam + " " + leerling.familieNaam);
                geschrevenMessages.Add("leerling " + i + "/" + distinctLeerlingen.Count + " is toegevoegd");
                await _leerlingService.Create(leerling);
                i++;
            }


            /*!!!     Opmaken van alle vakken en leerkrachten en de relaties met de klas .leerlingen gebruikt om de klas
                          te kunnen linken aan de vakken omdat de data niet consistent genoeg is in de soap response          !!!*/

            foreach (XElement element in xDoc.Descendants("courseTeacherClass"))
            {
                Console.WriteLine(element);
                stamboekenList.Clear();

                var stamboekNummer = 0L;
                var dbLeerling = new Leerling();

                //voor elke leerling van het vak worden de id bijgehouden en gebruikt om de klas uit de db te halen
                foreach (var leerling in element.Descendants("leerling"))
                {
                    stamboekNummer = Int64.Parse(leerling.Element("stamboeknummer").Value);
                    stamboekenList.Add(stamboekNummer);
                    Console.WriteLine(stamboekNummer);
                }

                //indien er geen id's gevonden zijn en dus geen leerlingen in de klas
                if (stamboekenList.Count != 0)
                {
                    dbLeerling = await _leerlingService.GetToImport(stamboekenList[0]);
                    if (dbLeerling == null)
                    {
                        dbLeerling = await _leerlingService.GetToImport(stamboekenList[1]);
                    }

                    if (element.Element("stamboeknummer").Value.Equals("NULL"))
                    {
                        var number = _leerkrachtService.Find().Result.Count(l => l.leerkrachtID > 0 && l.leerkrachtID < 100);
                        var soapVak = new Vak()
                        {
                            leerkrachtID = number + 1,
                            vaknaam = element.Element("vaknaam").Value,
                            klasID = dbLeerling.KlasID,
                            sprint = true
                        };
                        vakken.Add(soapVak);
                    }
                    else
                    {
                        var soapVak = new Vak()
                        {
                            leerkrachtID = long.Parse(element.Element("stamboeknummer").Value),
                            vaknaam = element.Element("vaknaam").Value,
                            klasID = dbLeerling.KlasID,
                            sprint = true
                        };
                        vakken.Add(soapVak);
                    }
                }
                else
                {
                    Console.WriteLine("leerlingen lijst leeg van deze klas ");
                }

                if (element.Element("stamboeknummer").Value.Equals("NULL"))
                {
                    var number = _leerkrachtService.Find().Result.Count(l => l.leerkrachtID > 0 && l.leerkrachtID < 1000);
                    var soapVakLeerkracht = new Leerkracht()
                    {
                        leerkrachtID = number + idMaker,
                        achternaam = element.Element("naam").Value,
                        voornaam = element.Element("voornaam").Value,
                        email = element.Element("gebruikersnaam").Value,
                        sprintToezichter = false,
                        status = true,
                        rol = 2
                    };
                    vakleerkrachten.Add(soapVakLeerkracht);
                    idMaker++;
                }
                else
                {
                    var soapVakLeerkracht = new Leerkracht()
                    {
                        leerkrachtID = long.Parse(element.Element("stamboeknummer").Value),
                        achternaam = element.Element("naam").Value,
                        voornaam = element.Element("voornaam").Value,
                        email = element.Element("gebruikersnaam").Value,
                        sprintToezichter = false,
                        status = true,
                        rol = 2
                    };
                    vakleerkrachten.Add(soapVakLeerkracht);
                }
            }
            var distinctLeerkrachten = vakleerkrachten.Distinct().ToList();

            i = 1;
            foreach (var leerkracht in distinctLeerkrachten)
            {
                geschrevenMessages.Add(leerkracht.voornaam + " " + leerkracht.achternaam);
                geschrevenMessages.Add("leerkracht " + i + "/" + distinctLeerkrachten.Count + " is toegevoegd");
                await _leerkrachtService.Create(leerkracht);
                i++;
            }
            var vakSchoonmaak = new Vak { vaknaam = "schoonmaak", klasID = 1, leerkrachtID = 1 };
            await _vakService.Create(vakSchoonmaak);
            i = 1;
            foreach (var vak in vakken)
            {
                //vak.Klas=klassen.SingleOrDefault()
                geschrevenMessages.Add(vak.vaknaam + " klasid: " + vak.klasID + " leerkrachtID: " + vak.leerkrachtID);
                geschrevenMessages.Add("Vak " + i + "/" + vakken.Count + " is toegevoegd");
                await _vakService.Create(vak);
                i++;
            }

            return PartialView("PartialBerichtenResults", geschrevenMessages);
        }




        /*!!!!!!!!!!!!  Importeren van start gegevens   !!!!!!!!!!!
          !                  XLS Lokalen import                   ! 
          !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!     Importeren van XLS en het gebruiken van
          !                 XLS van CSV uit MyRo                  !
          !                Examenrooster Import                   !     Berichten weergave via partial en ajax call
          !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/

        public async Task<IActionResult> XlsUpload(IFormFile xlsFile)
        {
            IList<Lokaal> lokalen = new List<Lokaal>();
            List<string> berichten = new List<string>();
            if (xlsFile != null)
            {
                var xlsStream = xlsFile.OpenReadStream();
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                using (var reader = ExcelReaderFactory.CreateReader(xlsStream))
                {
                    do
                    {
                        while (reader.Read()) //Each ROW
                        {
                            Lokaal lokaal = new Lokaal();
                            for (int column = 0; column < reader.FieldCount; column++)
                            {
                                if (column == 0) //Lokaalnaam
                                {
                                    lokaal.lokaalnaam = reader.GetValue(column).ToString(); //Get Value returns object
                                }
                                else if (column == 1) //afkorting
                                {
                                    lokaal.naamafkorting =
                                        reader.GetValue(column).ToString(); //Get Value returns object
                                }
                            }
                            lokalen.Add(lokaal);
                        }
                    } while (reader.NextResult()); //Move to NEXT SHEET
                }
                foreach (var lokaal in lokalen)
                {
                    if (!lokaal.lokaalnaam.Equals("lokaalnaam"))
                    {
                        await _lokaalService.CreateAsync(lokaal);
                        berichten.Add(lokaal.lokaalnaam + " " + lokaal.naamafkorting + " is created");
                    }
                }
                return View("ImportPagina", berichten);
            }
            berichten.Add("Opgelet! U heeft Geen bestand verzonden");
            return View("ImportPagina", berichten);
        }

        public async Task<IActionResult> ImportExamens()
        {
            IList<Examenrooster> examenroosters = new List<Examenrooster>();
            List<string> berichten = new List<string>();
            IFormFile file;
            if (Request.Form.Files.Count != 0)
            {
                file = Request.Form.Files[0];
                var xlsStream = file.OpenReadStream();
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                using (var reader = ExcelReaderFactory.CreateReader(xlsStream))
                {
                    do
                    {
                        while (reader.Read()) //Each ROW
                        {
                            var rooster = new Examenrooster();
                            var klas = new Klas();
                            var lokaal = new Lokaal();
                            var vak = new Vak();
                            //IList<Vak> vakken;

                            for (int column = 0; column < reader.FieldCount; column++)
                            {
                                if (column == 1) //Klas uit id
                                {
                                    var klasnaamUitXls = reader.GetValue(column).ToString();

                                    if (klasnaamUitXls.Substring(klasnaamUitXls.Length - 3, 3) == "gr1" ||
                                        (klasnaamUitXls.Substring(klasnaamUitXls.Length - 3, 3) == "gr2"))
                                    {
                                        Console.WriteLine(klasnaamUitXls.Substring(klasnaamUitXls.Length - 3, 3));
                                        var splitNaam = klasnaamUitXls.Split("g");
                                        klas = await _klasService.GetByKlasName(splitNaam[0]);
                                        rooster.groep = klasnaamUitXls.Substring(klasnaamUitXls.Length - 3, 3);

                                    }
                                    else
                                    {
                                        klas = await _klasService.GetByKlasName(reader.GetValue(column).ToString());
                                    }

                                    if (klas == null)
                                    {
                                        klas = await _klasService.GetAsync(1);
                                    }

                                    Console.WriteLine(reader.GetValue(column).ToString());
                                }

                                else if (column == 3) // vak naam met klas.id dat vakID geeft
                                {
                                    if (reader.GetValue(column).ToString().Contains("SCHOONMAAK")
                                    ) // opvangen van poets tijd
                                    {
                                        rooster.Vak = await _vakService.GetBySubString(reader.GetValue(column).ToString(), 1);
                                    }
                                    else if (reader.GetValue(column).ToString().Contains("MAVO"))
                                    {
                                        rooster.Vak = await _vakService.GetBySubString("Maatschapp", klas.klasID);
                                        Console.WriteLine(reader.GetValue(column) + " is gevonden");
                                    }
                                    else if (reader.GetValue(column).ToString().Contains("PSYCHOLOGIE"))
                                    {
                                        rooster.Vak = await _vakService.GetBySubString("Beroeps", klas.klasID);
                                        Console.WriteLine(reader.GetValue(column) +
                                                          "Beroepsgerichte psychologie is gelukt ");
                                    }
                                    else if (reader.GetValue(column).ToString().Contains("R EN E"))
                                    {
                                        rooster.Vak = await _vakService.GetBySubString("Religie", klas.klasID);
                                        Console.WriteLine(reader.GetValue(column) + "Religie en ethiek is gelukt ");
                                    }
                                    else if (reader.GetValue(column).ToString().Contains("IO3") ||
                                             reader.GetValue(column).ToString().Contains("IO4") ||
                                             reader.GetValue(column).ToString().Contains("IO5") ||
                                             reader.GetValue(column).ToString().Contains("IO6"))

                                    {
                                        if (klas.klasID != 1
                                        ) // laatste van de input io opvangen die geen vak heeft nog klas
                                        {
                                            rooster.Vak = await _vakService.GetBySubString("integrale opdr", klas.klasID);
                                            Console.WriteLine(reader.GetValue(column) +
                                                              "Integrale opdrachten tis gelukt ");
                                        }
                                        else
                                        {
                                            rooster.examenID = 999;  // Soort fault code om zo deze niet toe te voegen
                                            Console.WriteLine(reader.GetValue(column) + "Skip van de onnodige informatie op het einde van het document ");
                                        }
                                    }
                                    //TODO Nog nakijken wat deze juist zijn 

                                    else if (reader.GetValue(column).ToString().Contains("INSTROOM") || // OKAN Weten niet wat het juist is  maar normaal taal
                                             reader.GetValue(column).ToString().Contains("FOUTLOOS") || // OKAN TAAL examen
                                             reader.GetValue(column).ToString().Contains("MELKWEG") || //OKAN TAAL MELKWEG (25/02) examen
                                            reader.GetValue(column).ToString().Contains("NIEUW SPREEKRECHT") ||
                                             reader.GetValue(column).ToString().Contains("STUDIE") || // GEwoon studie om op school te blijven 
                                             reader.GetValue(column).ToString().Contains("SPRINT") || // voor sprint 
                                             reader.GetValue(column).ToString().Contains("RESERVE") || // geen nut 
                                             reader.GetValue(column).ToString().Contains("FG")) // OKAN TAAL examen
                                    {
                                        rooster.examenID = 999;  // Soort fault code om zo deze niet toe te voegen
                                        Console.WriteLine(reader.GetValue(column) + "Skip van de onnodige  op het einde van het document ");
                                    }
                                    else
                                    {
                                        rooster.Vak = await _vakService.GetBySubString(reader.GetValue(column).ToString(),
                                            klas.klasID);
                                        Console.WriteLine(reader.GetValue(column).ToString());
                                    }
                                }

                                //else if (column == 4) // lokaal ID nog verwerken in examenrooster
                                //{
                                //}

                                else if (column == 5) // datum van examen
                                {
                                    var splitPieceDatum = reader.GetValue(column).ToString().Split(" ")[1];
                                    DateTime date = DateTime.ParseExact(splitPieceDatum, "dd/MM/yyyy", null);
                                    rooster.datum = date;
                                    Console.WriteLine("datum :" + date);
                                } // datum van examen

                                else if (column == 6) // foutieve datum gevolgd van " " en het juiste uur opgevangen door split en item[1] van de result array
                                {
                                    var tweeDeligAntw = reader.GetValue(column).ToString().Split(" ");
                                    rooster.tijd = tweeDeligAntw[1];
                                    Console.WriteLine("uur :" + tweeDeligAntw[1]);
                                } // foutieve datum gevolgd van " " en het juiste uur opgevangen door split en item[1] van de result array
                            }
                            if (rooster.examenID != 999) // fault code van onnodige gegevens om deze niet te versturen en zo de database te sparen
                            {
                                rooster.vakID = rooster.Vak.vakID;
                                Console.WriteLine(rooster.Vak.vaknaam + " " + rooster.Vak.klasID + " " + rooster.datum);
                                Console.WriteLine("");
                                examenroosters.Add(rooster);
                            }
                        }
                    } while (reader.NextResult());
                }

                foreach (var rooster in examenroosters)
                {
                    await _examenroosterService.Create(rooster);
                    berichten.Add("examen op " + rooster.tijd + " " + rooster.datum + " met id" + rooster.examenID +
                                  " is aangemaakt");

                }
                return View("ImportPagina", berichten);
            }

            berichten.Add("Opgelet! U heeft Geen bestand verzonden");
            return View("ImportPagina", berichten);
        }



        /*!!!!!!!!!!!!     Beheren van  gegevens      !!!!!!!!!!!
        !       Leerkr, Leerl, Klas, Vak, Lokalen en klassen    ! 
        !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!  
        !                                                       !
        !                 Beheer Leerling                       !  werkend met paging 
        !                                                       !     //Nieuwe en efficientere manier met pages veel snellere respons en beter in meerdere opzichten
        !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/




        public async Task<IActionResult> LeerlingBeheer(string sortOrder, string currentFilter, string searchString, string search2String, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["KlasSortParm"] = sortOrder == "klas" ? "klas_desc" : "klas";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            ViewData["nameFilter"] = searchString;
            ViewData["klasFilter"] = search2String;

            var students = _leerlingService.FindAsyncPagingQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                students = students.Where(s => s.familieNaam.ToLower().Contains(searchString.ToLower())
                                              || s.voorNaam.ToLower().Contains(searchString.ToLower()));
            }

            if (!string.IsNullOrEmpty(search2String))
            {
                students = students.Where(s => s.Klas.klasnaam.ToLower().Contains(search2String.ToLower()));
            }


            switch (sortOrder)
            {
                case "name_desc":
                    students = students.OrderByDescending(s => s.familieNaam);
                    break;
                case "klas":
                    students = students.OrderBy(s => s.Klas.klasnaam);
                    break;
                case "klas_desc":
                    students = students.OrderByDescending(s => s.Klas.klasnaam);
                    break;
                default:
                    students = students.OrderBy(s => s.familieNaam);
                    break;
            }


            return View(await PaginatedList<Leerling>.CreateAsync(students.AsQueryable(), pageNumber ?? 1, 12));
        }


        public async Task<IActionResult> LeerlingDetails(int id)
        {
            var leerling = await _leerlingService.Get(id);
            return View("LeerlingDetails", leerling);
        }


        [HttpGet] // get van de gekozen leerling
        public async Task<IActionResult> LeerlingEdit(int id)
        {
            var leerling = await _leerlingService.Get(id);
            return View(leerling);
        }


        [HttpPost] // Post van de wijziging
        public async Task<IActionResult> LeerlingEdit(Leerling leerling)
        {
            await _leerlingService.Update(leerling.leerlingID, leerling);

            return RedirectToAction("LeerlingBeheer");
        }



        /* !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!  
           !                 Beheer Leerkracht                     ! werkend met paging
           !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/


        public async Task<IActionResult> LeerkrachtBeheer(string sortOrder, string currentFilter, string searchString, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["aantal"] = sortOrder == "aantal" ? "aantal_desc" : "aantal";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            ViewData["nameFilter"] = searchString;

            var leerkrachten = await _leerkrachtService.FindAsyncPagingQueryable();

            if (!String.IsNullOrEmpty(searchString))
            {
                leerkrachten = leerkrachten.Where(s => s.achternaam.ToLower().Contains(searchString.ToLower())
                                               || s.voornaam.ToLower().Contains(searchString.ToLower()));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    leerkrachten = leerkrachten.OrderByDescending(s => s.achternaam);
                    break;
                case "aantal":
                    leerkrachten = leerkrachten.OrderBy(s => s.Vakken.Count);
                    break;
                case "aantal_desc":
                    leerkrachten = leerkrachten.OrderByDescending(s => s.Vakken.Count);
                    break;
                default:
                    leerkrachten = leerkrachten.OrderBy(s => s.achternaam);
                    break;
            }


            return View(await PaginatedList<Leerkracht>.CreateAsync(leerkrachten.AsQueryable(), pageNumber ?? 1, 12));
        }

        public async Task<IActionResult> LeerkrachtDetails(long id)
        {
            var leerkracht = await _leerkrachtService.Get(id);
            return View("LeerkrachtDetails", leerkracht);
        }

        [HttpGet]
        public async Task<IActionResult> LeerkrachtEdit(long id)
        {
            if (id == 0)
            {
                return RedirectToAction("LeerkrachtBeheer");
            }
            var leerkracht = await _leerkrachtService.Get(id);
            return View(leerkracht);
        }


        [HttpPost]
        public async Task<IActionResult> LeerkrachtEdit(Leerkracht leerkracht)
        {
            await _leerkrachtService.Update(leerkracht.leerkrachtID, leerkracht);
            return RedirectToAction("LeerkrachtBeheer");
        }




        /* !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!  
           !                     Beheer Vak                        ! werkend met paging
           !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/

        public async Task<IActionResult> VakBeheer(string sortOrder, string currentFilter, string searchString, string search2String, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["KlasSortParm"] = sortOrder == "aantal" ? "id_desc" : "id";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            ViewData["nameFilter"] = searchString;
            ViewData["klasFilter"] = search2String;
            var vakken = await _vakService.FindAsyncPagingQueryable();

            if (!String.IsNullOrEmpty(searchString))
            {
                vakken = vakken.Where(s => s.vaknaam.ToLower().Contains(searchString.ToLower()));
            }
            if (!String.IsNullOrEmpty(search2String))
            {
                vakken = vakken.Where(s => s.klas.klasnaam.ToLower().Contains(search2String.ToLower()));
            }
            switch (sortOrder)
            {
                case "name_desc":
                    vakken = vakken.OrderByDescending(s => s.vaknaam);
                    break;
                case "id":
                    vakken = vakken.OrderBy(s => s.klasID);
                    break;
                case "id_desc":
                    vakken = vakken.OrderByDescending(s => s.klasID);
                    break;
                default:
                    vakken = vakken.OrderBy(s => s.vaknaam);
                    break;
            }


            return View(await PaginatedList<Vak>.CreateAsync(vakken.AsQueryable(), pageNumber ?? 1, 12));
        }

        public async Task<IActionResult> VakDetails(int id)
        {
            var vak = await _vakService.GetAsync(id);
            return View("VakDetails", vak);
        }

        [HttpGet]
        public async Task<IActionResult> VakEdit(int id)
        {
            if (id == 0)
            {
                return RedirectToAction("VakBeheer");
            }
            var vak = await _vakService.GetAsync(id);
            return View(vak);
        }

        [HttpPost]
        public async Task<IActionResult> VakEdit(Vak vak)
        {
            await _vakService.UpdateAsync(vak.vakID, vak);

            return RedirectToAction("VakBeheer");
        }


        /* !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!  Compleet async en LinQ => 1 query
           !                     Beheer Klas                        ! werkend met paging
           !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/



        public async Task<IActionResult> KlasBeheer(string sortOrder, string currentFilter, string searchString, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            ViewData["nameFilter"] = searchString;

            var klassen = await _klasService.FindAsyncPagingQueryable();

            if (!String.IsNullOrEmpty(searchString))
            {
                klassen = klassen.Where(k => k.klasnaam.ToLower().Contains(searchString.ToLower()));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    klassen = klassen.OrderByDescending(k => k.klasnaam);
                    break;
                default:
                    klassen = klassen.OrderBy(k => k.klasnaam);
                    break;
            }


            return View(await PaginatedList<Klas>.CreateAsync(klassen.AsQueryable(), pageNumber ?? 1, 12));
        }


        public async Task<IActionResult> KlasDetails(int id)
        {
            var leerling = await _klasService.GetAsync(id);
            return View("KlasDetails", leerling);
        }


        [HttpGet]
        public async Task<IActionResult> KlasEdit(int id)
        {
            if (id == 0)
            {
                return RedirectToAction("KlasBeheer");
            }
            var klas = await _klasService.GetAsync(id);
            return View(klas);
        }


        [HttpPost]
        public async Task<IActionResult> KlasEdit(Klas klas)
        {
            await _klasService.UpdateAsync(klas.klasID, klas);
            return RedirectToAction("KlasBeheer");
        }




        /* !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!  HttpGet AJAX
           !                     Beheer Lokaal                        ! 
           !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/



        public async Task<IActionResult> LokaalBeheer(string sortOrder, string currentFilter, string searchString, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            ViewData["nameFilter"] = searchString;

            var lokalen = _lokaalService.FindAsyncPagingQueryable();

            if (!String.IsNullOrEmpty(searchString))
            {
                lokalen = lokalen.Where(s => s.lokaalnaam.ToLower().Contains(searchString.ToLower()));

            }

            switch (sortOrder)
            {
                case "name_desc":
                    lokalen = lokalen.OrderByDescending(s => s.lokaalnaam);
                    break;
                default:
                    lokalen = lokalen.OrderBy(s => s.lokaalID);
                    break;
            }


            return View(await PaginatedList<Lokaal>.CreateAsync(lokalen.AsQueryable(), pageNumber ?? 1, 12));
        }

        [HttpGet]
        public async Task<IActionResult> LokaalDetails(int id)
        {
            var lokaal = await _lokaalService.GetAsync(id);
            return View("LokaalDetails", lokaal);
        }


        [HttpGet]
        public async Task<IActionResult> LokaalEdit(int id)
        {
            if (id == 0)
            {
                return RedirectToAction("LokaalBeheer");
            }
            var lokaal = await _lokaalService.GetAsync(id);
            return View(lokaal);
        }

        [HttpPost]
        public async Task<IActionResult> LokaalEdit(Lokaal lokaal)
        {
            await _lokaalService.UpdateAsync(lokaal.lokaalID, lokaal);
            return RedirectToAction("lokaalBeheer");
        }


        [HttpGet]
        public async Task<IActionResult> LokaalDelete(int id)
        {
            var lokaal = await _lokaalService.GetAsync(id);

            if (lokaal == null)
            {
                return RedirectToAction("LokaalBeheer");
            }

            return View(lokaal);
        }

        [HttpPost]
        [Route("/BeheerderArea/Beheerder/LokaalDelete/{id?}")]
        public async Task<IActionResult> LokaalDeleteConf(int id)
        {
            await _lokaalService.Delete(id);

            return RedirectToAction("LokaalBeheer");
        }



        /*!!!!!!!!!!!!         Extra functies           !!!!!!!!!!!
          !                                                       ! 
          !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/

        //Soap Connectie aanmaken om de data uit smartschool soap api te krijgen
        public V3PortClient SoapConnection()
        {
            var SoapSSApi = new V3PortClient();
            SoapSSApi.Endpoint.Address = new EndpointAddress("https://tihf.smartschool.be/Webservices/V3");

            return SoapSSApi;
        }

        //Om een object leerkracht te maken uit de soap respons object
        public Leerkracht SoapNaarleerkrachtmaker(TitularisEnKlasSoap soapLeerkracht)
        {
            Leerkracht leerkracht = new Leerkracht
            {
                leerkrachtID = long.Parse(soapLeerkracht.stamboeknummer),
                voornaam = soapLeerkracht.voornaam,
                achternaam = soapLeerkracht.naam,
                email = soapLeerkracht.gebruikersnaam,
                rol = 2,
                status = true,
                sprintToezichter = false
            };
            return leerkracht;
        }

        //Om een object klas te maken uit de soap respons object
        public Klas KlasMaker(TitularisEnKlasSoap klasSoap)
        {
            Klas klasMetTitul = new Klas
            {
                klasID = Int32.Parse(klasSoap.klasid),
                klasnaam = klasSoap.klasnaam,
                titularisID = Int64.Parse(klasSoap.stamboeknummer),
            };
            return klasMetTitul;
        }


    }

}
