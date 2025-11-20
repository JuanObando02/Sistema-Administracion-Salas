using Microsoft.AspNetCore.Mvc;
using MvcSample.Models;
using Services;
using System.Diagnostics;

namespace MvcSample.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ISalaService _salaService;

        public HomeController(ILogger<HomeController> logger, ISalaService salaService)
        {
           
            _logger = logger;
            _salaService = salaService;
        }
        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated)
            { 
                return View("Welcome");
            }

            var estadosSalas = await _salaService.GetEstadoActualSalas();
            return View(estadosSalas);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
