using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SprintPlannerZM.Ui.Mvc.Models;
using System.Diagnostics;

namespace SprintPlannerZM.Ui.Mvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult AdminIndex()
        {
            return RedirectToAction("Index", "Admin", new { area = "Admin" });
        }
    }
}
