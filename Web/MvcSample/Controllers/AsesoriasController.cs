using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.Models.AsesoriaModels;
using System.Security.Claims;

namespace MvcSample.Controllers
{
    [Authorize]
    public class AsesoriasController : Controller
    {
        private readonly IAsesoriaService _asesoriaService;

        public AsesoriasController(IAsesoriaService asesoriaService)
        {
            _asesoriaService = asesoriaService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var lista = await _asesoriaService.GetMisAsesorias(userId);
            return View(lista);
        }

        [HttpGet]
        public async Task<IActionResult> Solicitar(Guid? salaId)
        {
            var model = await _asesoriaService.GetDatosParaRegistrar(salaId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Solicitar(RegistrarAsesoriaModel model)
        {
            if (!ModelState.IsValid)
            {
                var recargado = await _asesoriaService.GetDatosParaRegistrar();
                model.SalasDisponibles = recargado.SalasDisponibles;
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            try
            {
                await _asesoriaService.CrearAsesoria(model, userId);
                TempData["Mensaje"] = "Solicitud enviada. Un coordinador vendrá en camino.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error al crear solicitud.");
                var recargado = await _asesoriaService.GetDatosParaRegistrar();
                model.SalasDisponibles = recargado.SalasDisponibles;
                return View(model);
            }
        }
    }
}
