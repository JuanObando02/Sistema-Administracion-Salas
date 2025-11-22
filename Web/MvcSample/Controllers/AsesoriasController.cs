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
        
        [HttpGet]
        [Authorize(Roles = "Coordinador, Admin")]
        public async Task<IActionResult> Gestionar()
        {
            var lista = await _asesoriaService.GetAsesoriasGestionar();
            return View(lista);
        }
        
        [HttpPost]
        [Authorize(Roles = "Coordinador, Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Atender(Guid id)
        {
            var coordId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                await _asesoriaService.MarcarEnProceso(id, coordId);
                TempData["Mensaje"] = "Asesoría marcada en proceso.";
            }
            catch (Exception ex) { TempData["Error"] = ex.Message; }
            return RedirectToAction("Gestionar");
        }

        [HttpPost]
        [Authorize(Roles = "Coordinador, Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Finalizar(Guid id, string? observaciones) // <-- Recibe observaciones
        {
            var coordId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                await _asesoriaService.FinalizarAsesoria(id, coordId, observaciones);
                TempData["Mensaje"] = "Asesoría finalizada con éxito.";
            }
            catch (Exception ex) { TempData["Error"] = ex.Message; }
            return RedirectToAction("Gestionar");
        }

        [HttpPost]
        [Authorize(Roles = "Coordinador, Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NoAplica(Guid id, string? observaciones) // <-- Recibe observaciones
        {
            var coordId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                await _asesoriaService.DescartarAsesoria(id, coordId, observaciones);
                TempData["Mensaje"] = "Solicitud cerrada como No Aplica.";
            }
            catch (Exception ex) { TempData["Error"] = ex.Message; }
            return RedirectToAction("Gestionar");
        }
        [HttpGet]
        [Authorize(Roles = "Coordinador, Admin")]
        public async Task<IActionResult> Historial(string? busqueda,Domain.Enums.EstadoAsesoria? estado,DateTime? fecha,int pagina = 1)
        {
            var filtro = new FiltroAsesoriaModel
            {
                Busqueda = busqueda,
                Estado = estado,
                Fecha = fecha,
                Pagina = pagina
            };

            var listaPaginada = await _asesoriaService.GetHistorialAsesorias(filtro);

            // Guardar estado de filtros para la vista
            ViewData["BusquedaActual"] = busqueda;
            ViewData["EstadoActual"] = estado;
            ViewData["FechaActual"] = fecha?.ToString("yyyy-MM-dd");

            return View(listaPaginada);
        }
    }
}
