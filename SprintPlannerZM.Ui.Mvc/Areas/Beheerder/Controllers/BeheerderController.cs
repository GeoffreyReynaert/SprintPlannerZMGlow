using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using SmartSchoolSoapApi;
using SoapSSMvc.Model;
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


        public BeheerderController(AppSettings appSettings, ILeerkrachtService leerkrachtService,
            IKlasService klasService, ILeerlingService leerlingService, IVakService vakService)
        {
            _appSettings = appSettings;
            _leerkrachtService = leerkrachtService;
            _klasService = klasService;
            _leerlingService = leerlingService;
            _vakService = vakService;
        }


        public async Task<IActionResult> Index()
        {
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> TeacherTest()
        {
            var SoapSSApi = SoapConnection();

            var result = await SoapSSApi.getClassTeachersAsync(
                _appSettings.SsApiPassword, true);

            IList<LeerkrachtSsSoap> leerkrachten = new List<LeerkrachtSsSoap>();

            JArray teachers = JArray.Parse(result.ToString());
            foreach (var teacher in teachers)
            {
                LeerkrachtSsSoap leerkracht = new LeerkrachtSsSoap();
                leerkracht = teacher.ToObject<LeerkrachtSsSoap>();
                leerkrachten.Add(leerkracht);
            }
            return View(leerkrachten);
        }

        [HttpGet]
        public async void ImportKlasTitularisEnKlas()
        {
            IList<LeerkrachtSsSoap> titularisenMetKlas = new List<LeerkrachtSsSoap>();
            var iReplaceNullResponseToId = 1;

            var SoapSSApi = SoapConnection();
            var result = await SoapSSApi.getClassTeachersAsync(
                _appSettings.SsApiPassword, true);

            JArray users = JArray.Parse(result.ToString());
            foreach (var user in users)
            {
                Console.WriteLine(user);
                var leerkracht = user.ToObject<LeerkrachtSsSoap>();
                if (leerkracht.stamboeknummer=="NULL")
                {
                    leerkracht.stamboeknummer = iReplaceNullResponseToId.ToString();
                    iReplaceNullResponseToId++;
                }
                titularisenMetKlas.Add(leerkracht);

                Console.WriteLine(leerkracht.naam + " " + leerkracht.voornaam + " " + leerkracht.klasnaam + " " + leerkracht.stamboeknummer+"/"+leerkracht.internummer);
            }

            var i = 1;
            foreach (var leerkracht in titularisenMetKlas)
            {
                Leerkracht teacher = new Leerkracht
                {
                    leerkrachtID = long.Parse(leerkracht.stamboeknummer),
                    voornaam = leerkracht.voornaam,
                    achternaam = leerkracht.naam,
                    email = leerkracht.gebruikersnaam,
                    rol = 2,
                    status = true,
                    sprinttoezichter = false
                };

                Klas klasMetTitul = new Klas
                {
                    klasID = Int32.Parse(leerkracht.klasid),
                    klasnaam = leerkracht.klasnaam,
                    titularisID = teacher.leerkrachtID
                };

                _leerkrachtService.Create(teacher);
                _klasService.Create(klasMetTitul);

                Console.WriteLine("Leerkracht met ID " + teacher.leerkrachtID + " naam " + teacher.achternaam + " " + teacher.voornaam);
                Console.WriteLine(i + "/" + titularisenMetKlas.Count + "is toegevoegd");

                Console.WriteLine("klas met ID " + klasMetTitul.klasID + " klasnaam " +
                                  klasMetTitul.klasnaam + " met titularisID" + klasMetTitul.titularisID + " naam " +
                                  teacher.achternaam + " " + teacher.voornaam);
                Console.WriteLine(i + "/" + titularisenMetKlas.Count + "is toegevoegd");
                i++;

            }
            var unknownKlas = new Klas{klasnaam = "0", titularisID = 1, klasID = 1};
            _klasService.Create(unknownKlas);
        }
        //Importeren van leerlingen leerkrachten vakken en hun relatie naar mekaar en de klas


        [HttpGet]
        public async void ImportStudentklasLeerkrachtVak()
        {
            var SoapSSApi = SoapConnection();

            var result = await SoapSSApi.getSkoreClassTeacherCourseRelationAsync(
                _appSettings.SsApiPassword);

            XDocument xDoc = XDocument.Parse(result.ToString());

            IList<Leerkracht> vakleerkrachten = new List<Leerkracht>();
            IList<Leerkracht> distinctLeerkrachten = new List<Leerkracht>();

            IList<Leerling> leerlingen = new List<Leerling>();
            IList<Leerling> distinctLeerlingen = new List<Leerling>();

            IList<Vak> vakken = new List<Vak>();
            
            foreach (XElement element in xDoc.Descendants("courseTeacherClass"))
            {
                if (element.Element("internnummer").Value != "")
                {
                    var dbKlas = _klasService.Get(element.Element("klasnaam").Value);


                    var soapVak = new Vak()
                    {
                        leerkrachtID = long.Parse(element.Element("stamboeknummer").Value),
                        vaknaam = element.Element("vaknaam").Value,
                        klasID =dbKlas.klasID};
                    vakken.Add(soapVak);

                    var soapVakLeerkracht = new Leerkracht()
                    {
                        leerkrachtID = long.Parse(element.Element("stamboeknummer").Value),
                        achternaam = element.Element("naam").Value,
                        voornaam = element.Element("voornaam").Value,
                        email = element.Element("gebruikersnaam").Value,
                        sprinttoezichter = false,
                        status = true,
                        rol = 2};

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
                        KlasID = dbKlas.klasID};
                    leerlingen.Add(soapLeerling);
                }
            }

            distinctLeerkrachten = vakleerkrachten.Distinct().ToList();
            distinctLeerlingen = leerlingen.Distinct().ToList();

            var i = 1;
            foreach (var leerling in distinctLeerlingen)
            {
                Console.WriteLine(leerling.voorNaam + " " + leerling.familieNaam);
                Console.WriteLine("leerling " + i + "/" + distinctLeerlingen.Count + " is toegevoegd");
                _leerlingService.Create(leerling);
                i++;
            }

            i = 1;
            foreach (var leerkracht in distinctLeerkrachten)
            {
                Console.WriteLine(leerkracht.voornaam + " " + leerkracht.achternaam);
                Console.WriteLine("leerkracht " + i + "/" + distinctLeerkrachten.Count + " is toegevoegd");
                _leerkrachtService.Create(leerkracht);
                i++;
            }

            i = 1;
            foreach (var vak in vakken)
            {
                Console.WriteLine(vak.vaknaam + " klasid: " + vak.klasID + " leerkrachtID: " + vak.leerkrachtID);
                Console.WriteLine("Vak " + i + "/" + vakken.Count + " is toegevoegd");
                _vakService.Create(vak);
                i++;
            }
        }


        //enigste manier om de fout te omzeilen dat het woord adress onbekend was in reference file van de connected service SOAP
        public V3PortClient SoapConnection()
        {
            
            var SoapSSApi = new V3PortClient();
            SoapSSApi.Endpoint.Address = new EndpointAddress("https://tihf.smartschool.be/Webservices/V3");

            return SoapSSApi;
        }
    }
}
