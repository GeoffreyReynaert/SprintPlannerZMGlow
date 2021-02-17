using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ExcelDataReader;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using SmartSchoolSoapApi;
using SprintPlannerZM.Model;
using SprintPlannerZM.Services.Abstractions;
using SprintPlannerZM.Ui.Mvc.Settings;

// ReSharper disable CommentTypo

namespace SprintPlannerZM.Ui.Mvc.Areas.Beheerder.Controllers
{
    [Area("Beheerder")]
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
            //opvangen soap fouten
            var  klas = new Klas(){klasID = 1,klasnaam = "0",titularisID = 1 };
            _klasService.Create(klas);
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
                    var dbKlas = _klasService.Get(element2.Parent.Parent.Element("klasnaam").Value);

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

                //indien er geen id's gevonden zijn 
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
                    var number = _leerkrachtService.Find().Count(l => l.leerkrachtID > 0 && l.leerkrachtID < 100);
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

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

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
            var vakSchoonmaak = new Vak { vaknaam = "schoonmaak", klasID = 1, leerkrachtID = 1};
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
                        IList<Vak> vakken;

                        for (int column = 0; column < reader.FieldCount; column++)
                        {
                            if (column == 1) //Klas uit id
                            {
                                klas = _klasService.GetBySubString(reader.GetValue(column).ToString());
                                Console.WriteLine(reader.GetValue(column).ToString());
                            }// Klasnaam

                            else if (column == 3) // vak naam met klas.id dat vakID geeft
                            {
                                if (reader.GetValue(column).ToString().Contains("SCHOONMAAK"))// opvangen van poets tijd
                                {
                                    vak = _vakService.GetBySubString(reader.GetValue(column).ToString(), 1);
                                }
                                else if (reader.GetValue(column).ToString().Contains("MAVO"))
                                {
                                    vak = _vakService.GetBySubString("maatschappelijke vorming", klas.klasID);
                                    Console.WriteLine(reader.GetValue(column) + " MAVO tis gelukt");
                                }
                                else if (reader.GetValue(column).ToString().Contains("AAR")) // kan nog hieronder bij de rest just for testing purposes
                                {
                                    vakken = _vakService.FindBySubstring(reader.GetValue(column).ToString(), klas.klasID);
                                    vak = vakken[0];
                                    Console.WriteLine(reader.GetValue(column) + "Aardrijkskunde tis gelukt");
                                }
                                else if (reader.GetValue(column).ToString().Contains("FRA")) // kan nog hieronder bij de rest just for testing purposes
                                {
                                    vakken = _vakService.FindBySubstring(reader.GetValue(column).ToString(), klas.klasID);
                                    vak = vakken[0];
                                    Console.WriteLine(reader.GetValue(column) + "frans tis gelukt");
                                }
                                else if (reader.GetValue(column).ToString().Contains("WIS")) // alles kan hierin later
                                {
                                    vakken = _vakService.FindBySubstring(reader.GetValue(column).ToString(), klas.klasID);
                                    vak = vakken[0];
                                    Console.WriteLine(reader.GetValue(column) + "wiskunde  tis gelukt ");
                                }
                                else
                                {
                                    vakken = _vakService.FindBySubstring(reader.GetValue(column).ToString(), klas.klasID);
                                    vak = vakken[0];
                                    Console.WriteLine(reader.GetValue(column).ToString());
                                }
                                rooster.vakID = vak.vakID;
                            } // vak naam met klas.id dat vakID geeft

                            else if (column == 4)// lokaal ID nog verwerken in examenrooster
                            {
                                lokaal = _lokaalService.Get(Int32.Parse(reader.GetValue(column).ToString()));
                                Console.WriteLine(Int32.Parse(reader.GetValue(column).ToString()));
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
