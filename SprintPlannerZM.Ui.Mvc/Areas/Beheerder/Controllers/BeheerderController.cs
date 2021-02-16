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


        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ImportPagina()
        {
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> ImportKlasTitularisEnKlas()
        {
            IList<TitularisEnKlasSoap> titularisenMetKlas = new List<TitularisEnKlasSoap>();
            IList<string> geschrevenResults = new List<string>();
            var index = 1;

            var SoapSSApi = SoapConnection();
            var result = await SoapSSApi.getClassTeachersAsync(_appSettings.SsApiPassword, true);

            JArray users = JArray.Parse(result.ToString());

            foreach (var user in users)
            {
                var leerkracht = user.ToObject<TitularisEnKlasSoap>();

                if (leerkracht.stamboeknummer == "NULL")
                {
                    leerkracht.stamboeknummer = index.ToString();
                    index++;
                }
              
                titularisenMetKlas.Add(leerkracht);

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

            //Gemaakt om de klas als result op te vangen !!fout in de data!!
            var unknownKlas = new Klas { klasnaam = "0", titularisID = 1, klasID = 1 };
            _klasService.Create(unknownKlas);

            return PartialView("PartialBerichtenResults", geschrevenResults);
        }


        //Importeren van leerlingen leerkrachten vakken en hun relatie naar mekaar en de klas
        [HttpGet]
        public async Task<IActionResult> ImportStudentklasLeerkrachtVak()
        {
            IList<string> geschrevenMessages = new List<string>();

            IList<Leerkracht> vakleerkrachten = new List<Leerkracht>();
            IList<Leerkracht> distinctLeerkrachten = new List<Leerkracht>();

            IList<Leerling> leerlingen = new List<Leerling>();
            IList<Leerling> distinctLeerlingen = new List<Leerling>();

            IList<Vak> vakken = new List<Vak>();

            var SoapSSApi = SoapConnection();

            var result = await SoapSSApi.getSkoreClassTeacherCourseRelationAsync(
                _appSettings.SsApiPassword);

            XDocument xDoc = XDocument.Parse(result.ToString());

            foreach (XElement element in xDoc.Descendants("courseTeacherClass"))
            {
                if (element.Element("internnummer").Value != "")
                {
                    var dbKlas = _klasService.Get(element.Element("klasnaam").Value);

                    var soapVak = new Vak()
                    {
                        leerkrachtID = long.Parse(element.Element("stamboeknummer").Value),
                        vaknaam = element.Element("vaknaam").Value,
                        klasID = dbKlas.klasID
                    };
                    vakken.Add(soapVak);

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

            foreach (XElement element2 in xDoc.Descendants("leerling"))
            {
                if (element2.Element("internnummer").Value != "")
                {
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

            distinctLeerkrachten = vakleerkrachten.Distinct().ToList();
            distinctLeerlingen = leerlingen.Distinct().ToList();

            var i = 1;
            foreach (var leerling in distinctLeerlingen)
            {
                geschrevenMessages.Add(leerling.voorNaam + " " + leerling.familieNaam);
                geschrevenMessages.Add("leerling " + i + "/" + distinctLeerlingen.Count + " is toegevoegd");
                _leerlingService.Create(leerling);
                i++;
            }

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
                geschrevenMessages.Add(vak.vaknaam + " klasid: " + vak.klasID + " leerkrachtID: " + vak.leerkrachtID);
                geschrevenMessages.Add("Vak " + i + "/" + vakken.Count + " is toegevoegd");
                _vakService.Create(vak);
                i++;
            }

            return PartialView("PartialBerichtenResults", geschrevenMessages);
        }


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

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            using (var reader = ExcelReaderFactory.CreateReader(xlsStream))
            {
                do
                {
                    while (reader.Read()) //Each ROW
                    {
                        Examenrooster rooster = new Examenrooster();
                        for (int column = 0; column < reader.FieldCount; column++)
                        {
                            if (column == 0) //Lokaalnaam
                            {
                                rooster.examenID =Int32.Parse( reader.GetValue(column).ToString());//Get Value returns object
                            }
                            //else if (column == 1) //afkorting
                            //{
                            //    rooster. = reader.GetValue(column).ToString();//Get Value returns object
                            //}
                        }
                        examenroosters.Add(rooster);
                    }
                } while (reader.NextResult()); //Move to NEXT SHEET
            }

            foreach (var rooster in examenroosters)
            {
              //  _lokaalService.Create(lokaal);
                    berichten.Add(rooster.examenID +" is created");

            }
            return View("ImportPagina", berichten);
 
        }
        

        //FUNCTIES//
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
