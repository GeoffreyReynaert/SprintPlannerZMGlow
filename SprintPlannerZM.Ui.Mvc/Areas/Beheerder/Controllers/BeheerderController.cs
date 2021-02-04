using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
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


        public BeheerderController(AppSettings appSettings,ILeerkrachtService leerkrachtService)
        {
            _appSettings = appSettings;
            _leerkrachtService = leerkrachtService;
        }



        public async Task<IActionResult> Index()
        {
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> TeacherTest()
        {
            var SoapSSApi = new V3PortClient();
            SoapSSApi.Endpoint.Address = new EndpointAddress("https://tihf.smartschool.be/Webservices/V3");

            var result = await SoapSSApi.getClassTeachersAsync(
                _appSettings.SsApiPassword, true);
            Console.WriteLine(result);

            IList<LeerkrachtSsSoap> leerkrachten = new List<LeerkrachtSsSoap>();
            JArray users = JArray.Parse(result.ToString());
            foreach (var user in users)
            {
                LeerkrachtSsSoap leerkracht = new LeerkrachtSsSoap();
                //{
                //    naam = user["naam"]; voornaam = user["voornaam"]...
                //};

                leerkracht = user.ToObject<LeerkrachtSsSoap>();
                leerkrachten.Add(leerkracht);
                Console.WriteLine(leerkracht.naam + " " + leerkracht.voornaam + " " + leerkracht.isOfficial + " " +
                                  leerkracht.klasnaam);

            }

            return View(leerkrachten);
        }

        public async void ImportTeachers()
        {

            var SoapSSApi = new V3PortClient();
            SoapSSApi.Endpoint.Address = new EndpointAddress("https://tihf.smartschool.be/Webservices/V3");

            var result = await SoapSSApi.getClassTeachersAsync(
                _appSettings.SsApiPassword, true);
            Console.WriteLine(result);

            IList<LeerkrachtSsSoap> leerkrachten = new List<LeerkrachtSsSoap>();
            JArray users = JArray.Parse(result.ToString());
            foreach (var user in users)
            {
                LeerkrachtSsSoap leerkracht = new LeerkrachtSsSoap();
                //{
                //    naam = user["naam"]; voornaam = user["voornaam"]...
                //};

                leerkracht = user.ToObject<LeerkrachtSsSoap>();
                leerkrachten.Add(leerkracht);
                Console.WriteLine(leerkracht.naam + " " + leerkracht.voornaam + " " + leerkracht.isOfficial + " " +
                                  leerkracht.klasnaam);
            }

            var i = 1;
            foreach (var leerkracht in leerkrachten)
            {
                
                Leerkracht teacher = new Leerkracht
                {
                    leerkrachtID = i,
                    voornaam = leerkracht.voornaam,
                    achternaam = leerkracht.naam,
                    email = leerkracht.gebruikersnaam,
                    rol = 2,
                    status = true,
                    sprinttoezichter = false
                };

                _leerkrachtService.Create(teacher);
                Console.WriteLine(i+"/"+leerkrachten.Count+" Have been inserted");
                i++;
            }

        }
    }
}
