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

        public IActionResult BeherenGegevens()
        {
            return View();
        }



        /*!!!!!!!!!!!!  Importeren van start gegevens   !!!!!!!!!!!
          !               Titularissen en klassen   ||            ! 
          !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!  Connectie met Soap Api
          !        klassen, Studenten, leerkrachten, vakken       !
          !           en de relatie die dezze verbind             !  Berichten weergave via partial en ajax call
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
            _leerkrachtService.Create(leerkacht);
            _klasService.Create(klas);

            var i = 1;
            foreach (var soapLeerkrachtEnKlas in titularisenMetKlas)
            {
                Leerkracht titularis = SoapNaarleerkrachtmaker(soapLeerkrachtEnKlas);
                Klas klasMetTitul = KlasMaker(soapLeerkrachtEnKlas);

                _leerkrachtService.Create(titularis);
                _klasService.Create(klasMetTitul);
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
                    var dbKlas = _klasService.GetByKlasName(element2.Parent.Parent.Element("klasnaam").Value);

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
                _leerlingService.Create(leerling);
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
                    stamboekNummer = Int64.Parse(leerling.Element("stamboeknummer").Value.ToString());
                    stamboekenList.Add(stamboekNummer);
                    Console.WriteLine(stamboekNummer);
                }

                //indien er geen id's gevonden zijn en dus geen leerlingen in de klas
                if (stamboekenList.Count != 0)
                {
                    dbLeerling = _leerlingService.Get(stamboekenList[0]);
                    if (dbLeerling == null)
                    {
                        dbLeerling = _leerlingService.Get(stamboekenList[1]);
                    }

                    if (element.Element("stamboeknummer").Value.Equals("NULL"))
                    {
                        var number = _leerkrachtService.Find().Count(l => l.leerkrachtID > 0 && l.leerkrachtID < 100);
                        var soapVak = new Vak()
                        {
                            leerkrachtID = number + 1,
                            vaknaam = element.Element("vaknaam").Value,
                            klasID = dbLeerling.KlasID
                        };
                        vakken.Add(soapVak);
                    }
                    else
                    {
                        var soapVak = new Vak()
                        {
                            leerkrachtID = long.Parse(element.Element("stamboeknummer").Value),
                            vaknaam = element.Element("vaknaam").Value,
                            klasID = dbLeerling.KlasID
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
                    var number = _leerkrachtService.Find().Count(l => l.leerkrachtID > 0 && l.leerkrachtID < 1000);
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
                _leerkrachtService.Create(leerkracht);
                i++;
            }

            i = 1;
            foreach (var vak in vakken)
            {
                //vak.Klas=klassen.SingleOrDefault()
                geschrevenMessages.Add(vak.vaknaam + " klasid: " + vak.klasID + " leerkrachtID: " + vak.leerkrachtID);
                geschrevenMessages.Add("Vak " + i + "/" + vakken.Count + " is toegevoegd");
                _vakService.Create(vak);
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
                                lokaal.lokaalnaam = reader.GetValue(column).ToString();//Get Value returns object
                            }
                            else if (column == 1) //afkorting
                            {
                                lokaal.naamafkorting = reader.GetValue(column).ToString();//Get Value returns object
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
                    _lokaalService.Create(lokaal);
                    berichten.Add(lokaal.lokaalnaam + " " + lokaal.naamafkorting + " is created");
                }

            }
            return View("ImportPagina", berichten);
        }

        public IActionResult ImportExamens()
        {
            IList<Examenrooster> examenroosters = new List<Examenrooster>();
            List<string> berichten = new List<string>();

            var file = Request.Form.Files[0];
            var xlsStream = file.OpenReadStream();
            var vakSchoonmaak = new Vak { vaknaam = "schoonmaak", klasID = 1, leerkrachtID = 1 };
            _vakService.Create(vakSchoonmaak);
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
                                klas = _klasService.GetBySubString(reader.GetValue(column).ToString());
                                if (klas==null)
                                {
                                    klas = _klasService.Get(1);
                                }
                                Console.WriteLine(reader.GetValue(column).ToString());
                            }

                            else if (column == 3) // vak naam met klas.id dat vakID geeft
                            {
                                if (reader.GetValue(column).ToString().Contains("SCHOONMAAK"))// opvangen van poets tijd
                                {
                                    rooster.Vak = _vakService.GetBySubString(reader.GetValue(column).ToString(), 1);
                                }
                                else if (reader.GetValue(column).ToString().Contains("MAVO"))
                                {
                                    rooster.Vak = _vakService.GetBySubString("Maatschapp", klas.klasID);
                                    Console.WriteLine(reader.GetValue(column) + " MAVO tis gelukt");
                                }
                                else if (reader.GetValue(column).ToString().Contains("SCHOOLTAAL")) // kan nog hieronder bij de rest just for testing purposes
                                {

                                    rooster.Vak = _vakService.GetBySubString(reader.GetValue(column).ToString(), klas.klasID);
                                    Console.WriteLine(reader.GetValue(column) + "Schooltaal tis gelukt");
                                }
                                else if (reader.GetValue(column).ToString().Contains("AA")) 
                                {
                                    //TODO Veranderen van AAR naar AA moet consitent blijven zoals bv 3 letters en moet vast liggen met charlotte 3 is nodig AA te weinig

                                    rooster.Vak = _vakService.GetBySubString("Aar", klas.klasID);
                                    Console.WriteLine(reader.GetValue(column) + "Aardrijkskunde tis gelukt");
                                }
                                else if (reader.GetValue(column).ToString().Contains("NAT.WET."))
                                {
                                    rooster.Vak = _vakService.GetBySubString(reader.GetValue(column).ToString(), klas.klasID);
                                    Console.WriteLine(reader.GetValue(column) + "nat.wet tis gelukt");
                                }
                                else if (reader.GetValue(column).ToString().Contains("FRA"))
                                {
                                    rooster.Vak = _vakService.GetBySubString(reader.GetValue(column).ToString(), klas.klasID);
                                    Console.WriteLine(reader.GetValue(column) + "frans tis gelukt");
                                }
                                else if (reader.GetValue(column).ToString().Contains("STD.V/D PUBL"))
                                {
                                    rooster.Vak = _vakService.GetBySubString("studie van de".ToString(), klas.klasID);
                                    Console.WriteLine(reader.GetValue(column) + "studie van de publiciteit tis gelukt");
                                }
                                else if (reader.GetValue(column).ToString().Contains("WIS")) 
                                {
                                    rooster.Vak = _vakService.GetBySubString(reader.GetValue(column).ToString(), klas.klasID);
                                    Console.WriteLine(reader.GetValue(column) + "wiskunde  tis gelukt ");
                                }
                                else if (reader.GetValue(column).ToString().Contains("KI"))
                                {
                                    rooster.Vak = _vakService.GetBySubString("kunst", klas.klasID);
                                    Console.WriteLine(reader.GetValue(column) + "kunst initiatie tis gelukt ");
                                }
                                else if (reader.GetValue(column).ToString().Contains("IO3") ||
                                         reader.GetValue(column).ToString().Contains("IO4") ||
                                         reader.GetValue(column).ToString().Contains("IO5") ||
                                         reader.GetValue(column).ToString().Contains("IO6")) 
                                {
                                    if (klas.klasID != 1) // laatste van de input io opvangen die geen vak heeft nog klas
                                    {
                                        rooster.Vak = _vakService.GetBySubString("integrale opdr", klas.klasID);
                                        Console.WriteLine(reader.GetValue(column) + "Integrale opdrachten tis gelukt ");
                                    }
                                    else
                                    {
                                        rooster.Vak = _vakService.GetBySubString("SCHOONMAAK", klas.klasID);
                                        Console.WriteLine(reader.GetValue(column) + "Integrale opdrachten tis gelukt ");
                                    }
                                }
                                else if (reader.GetValue(column).ToString().Contains("INSTROOM") ||
                                         reader.GetValue(column).ToString().Contains("FOUTLOOS") ||
                                         reader.GetValue(column).ToString().Contains("WELKWEG") ||
                                         reader.GetValue(column).ToString().Contains("NIEUW SPREEKRECHT") ||
                                         reader.GetValue(column).ToString().Contains("STUDIE")||
                                         reader.GetValue(column).ToString().Contains("SPRINT") ||
                                         reader.GetValue(column).ToString().Contains("RESERVE") ||
                                         reader.GetValue(column).ToString().Contains("FG")) //7de jaar vind het juiste vak niet 
                                {
                                    //TODO instroom bestaat niet als vak in smartschool wat is dit ? momenteel omzeild

                                    rooster.Vak = _vakService.GetBySubString("schoonmaak", 1);
                                    Console.WriteLine(reader.GetValue(column) + "Niet gekende vakken opgemaakt ");
                                }
                                else if (reader.GetValue(column).ToString().Contains("OAVI"))
                                {
                                    rooster.Vak = _vakService.GetBySubString("Ortho", klas.klasID);
                                    Console.WriteLine(reader.GetValue(column) + "ortho is gelukt ");
                                }
                                else if (reader.GetValue(column).ToString().Contains("PSYCHOLOGIE"))
                                {
                                    rooster.Vak = _vakService.GetBySubString("Beroeps", klas.klasID);
                                    Console.WriteLine(reader.GetValue(column) + "Beroepsgerichte psychologie is gelukt ");
                                }
                                else if (reader.GetValue(column).ToString().Contains("R EN E"))
                                {
                                    rooster.Vak = _vakService.GetBySubString("Religie", klas.klasID);
                                    Console.WriteLine(reader.GetValue(column) + "Religie en ethiek is gelukt ");
                                }
                                else
                                {
                                    rooster.Vak = _vakService.GetBySubString(reader.GetValue(column).ToString(), klas.klasID);
                                    Console.WriteLine(reader.GetValue(column).ToString());
                                }


                            } // vak naam met klas.id dat vakID geeft

                            else if (column == 4)// lokaal ID nog verwerken in examenrooster
                            {
                                if (reader.GetValue(column)!=null)
                                {
                                    lokaal = _lokaalService.GetByName(reader.GetValue(column).ToString());
                                    Console.WriteLine(reader.GetValue(column).ToString());
                                }
                                
                            } // lokaal ID nog verwerken in examenrooster

                            else if (column == 5)// datum van examen
                            {
                                rooster.datum = reader.GetValue(column).ToString();
                                Console.WriteLine("datum :" + reader.GetValue(column));
                            } // datum van examen

                            else if (column == 6)// foutieve datum gevolgd van " " en het juiste uur opgevangen door split en item[1] van de result array
                            {
                                var tweeDeligAntw = reader.GetValue(column).ToString().Split(" ");
                                rooster.tijd = tweeDeligAntw[1];
                                Console.WriteLine("uur :" + tweeDeligAntw[1]);
                            } // foutieve datum gevolgd van " " en het juiste uur opgevangen door split en item[1] van de result array
                        }
                        rooster.vakID = rooster.Vak.vakID;
                        Console.WriteLine(rooster.Vak.vaknaam + " " + rooster.Vak.klasID + " " + rooster.datum);
                        Console.WriteLine("");
                        examenroosters.Add(rooster);
                    }
                } while (reader.NextResult());
            }

            foreach (var rooster in examenroosters)
            {
                _examenroosterService.Create(rooster);
                berichten.Add("examen op " + rooster.tijd + " " + rooster.datum + " met id" + rooster.examenID + " is aangemaakt");
            }
            return View("ImportPagina", berichten);

        }




        /*!!!!!!!!!!!!     Beheren van  gegevens      !!!!!!!!!!!
        !       Leerkr, Leerl, Klas, Vak, Lokalen en klassen    ! 
        !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!  HttpGet AJAX
        !                                                       !
        !                 Beheer Leerling                       !  Berichten weergave via partial en ajax call
        !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/


        [HttpGet]  //get van de lijst
        public async Task<IActionResult> BeherenLeerling()
        {
            var leerlingen = _leerlingService.Find();

            return PartialView("PartialBeherenLeerling", leerlingen);
        }


        [HttpGet] // get van de gekozen item
        public IActionResult LeerlingEdit(int id)
        {
            //if (id == 0)
            //{
            //    return RedirectToAction("BeherenLeerling");
            //}
            var leerling = _leerlingService.Get(id);
            return View(leerling);
        }


        [HttpPost] // Post van de wijziging
        public IActionResult LeerlingEdit(Leerling leerling)
        {
            _leerlingService.Update(leerling.leerlingID, leerling);
            var berichten = new List<string> { "leerling " + leerling.familieNaam + " is gewijzigd" };

            return RedirectToActionPermanent("BeherenGegevens", berichten);

        }


        /* !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!  HttpGet AJAX
           !                 Beheer Leerkracht                     ! 
           !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/

        [HttpGet]
        public async Task<IActionResult> BeherenLeerkracht()
        {
            var leerkrachten = _leerkrachtService.Find();

            return PartialView("PartialBeherenLeerkracht", leerkrachten);
        }

        [HttpGet]
        public IActionResult LeerkrachtEdit(long id)
        {
            if (id == 0)
            {
                return RedirectToAction("BeherenLeerkracht");
            }
            var leerkracht = _leerkrachtService.Get(id);
            return View(leerkracht);
        }

        [HttpPost]
        public IActionResult LeerkrachtEdit(Leerkracht leerkracht)
        {
            _leerkrachtService.Update(leerkracht.leerkrachtID, leerkracht);
            var berichten = new List<string> { "leerkracht " + leerkracht.achternaam + " is gewijzigd" };
            return View("BeherenGegevens", berichten);
        }


        /* !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!  HttpGet AJAX
           !                     Beheer Vak                        ! 
           !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/

        [HttpGet]
        public async Task<IActionResult> BeherenVak()
        {
            var vakken = _vakService.Find();

            return PartialView("PartialBeherenVak", vakken);
        }

        [HttpGet]
        public IActionResult VakEdit(int id)
        {
            if (id == 0)
            {
                return RedirectToAction("BeherenVak");
            }
            var vak = _vakService.Get(id);
            return View(vak);
        }

        [HttpPost]
        public IActionResult VakEdit(Vak vak)
        {
            _vakService.Update(vak.vakID, vak);
            var berichten = new List<string> { "vak " + vak.vaknaam + " is gewijzigd" };
            return View("BeherenGegevens", berichten);
        }


        /* !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!  HttpGet AJAX
           !                     Beheer Klas                        ! 
           !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/



        [HttpGet]
        public async Task<IActionResult> BeherenKlas()
        {
            var klassen = _klasService.Find();

            return PartialView("PartialBeherenKlas", klassen);
        }

        [HttpGet]
        public IActionResult KlasEdit(int id)
        {
            if (id == 0)
            {
                return RedirectToAction("BeherenKlas");
            }
            var klas = _klasService.Get(id);
            return View(klas);
        }

        [HttpPost]
        public IActionResult KlasEdit(Klas klas)
        {
            _klasService.Update(klas.klasID, klas);
            var berichten = new List<string> { "klas " + klas.klasnaam + " is gewijzigd" };
            return View("BeherenGegevens", berichten);
        }




        /* !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!  HttpGet AJAX
           !                     Beheer Lokaal                        ! 
           !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/



        [HttpGet]
        public async Task<IActionResult> BeherenLokalen()
        {
            var lokalen = _lokaalService.Find();

            return PartialView("PartialBeherenLokalen", lokalen);
        }

        [HttpGet]
        public IActionResult LokaalEdit(int id)
        {
            if (id == 0)
            {
                return RedirectToAction("BeherenLokalen");
            }
            var lokaal = _lokaalService.Get(id);
            return View(lokaal);
        }

        [HttpPost]
        public IActionResult LokaalEdit(Lokaal lokaal)
        {
            _lokaalService.Update(lokaal.lokaalID, lokaal);
            var berichten = new List<string> { "lokaal " + lokaal.lokaalnaam + " is gewijzigd" };
            return View("BeherenGegevens", berichten);
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
