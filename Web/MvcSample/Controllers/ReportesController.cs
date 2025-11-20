using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.Models.ReporteModels;
using System.Security.Claims;

namespace MvcSample.Controllers
{
    [Authorize]
    public class ReportesController : Controller
    {
        private readonly IReporteService _reporteService;

        public ReportesController(IReporteService reporteService)
        {
            _reporteService = reporteService;
        }

        [HttpGet]
        public async Task<IActionResult> Registrar(Guid? salaId, Guid? equipoId) // <-- Acepta parámetros
        {
            // Pasamos los IDs al servicio
            var model = await _reporteService.GetDatosParaReportar(salaId, equipoId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(CrearReporteModel model)
        {
            if (!ModelState.IsValid)
            {
                var recargado = await _reporteService.GetDatosParaReportar();
                // (Si habías seleccionado equipo, aquí tendríamos que recargar esa lista también
                //  pero requeriría un poco más de lógica JS/AJAX para que sea perfecto).
                model.SalasDisponibles = recargado.SalasDisponibles;
                return View(model);
            }

            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            try
            {
                await _reporteService.CrearReporte(model, usuarioId);

                // Mensaje de éxito temporal usando TempData
                TempData["Mensaje"] = "¡Reporte enviado exitosamente! Un coordinador lo revisará pronto.";
                return RedirectToAction("MisReportes");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var recargado = await _reporteService.GetDatosParaReportar();
                model.SalasDisponibles = recargado.SalasDisponibles;
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetEquiposPorSala(Guid salaId)
        {
            var equipos = await _reporteService.GetEquiposPorSalaParaDropdown(salaId);
            return Json(equipos);
        }

        [HttpGet]
        public async Task<IActionResult> MisReportes()
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var misReportes = await _reporteService.GetMisReportes(usuarioId);

            return View(misReportes);
        }

    }
}
