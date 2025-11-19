using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using System.Security.Claims;
using Services.Models.ReservaModels;

namespace MvcSample.Controllers
{
    [Authorize(Roles = "Admin, Master, Estudiante, Profesor")]
    public class ReservasController : Controller
    {
        private readonly IReservaService _reservaService;

        public ReservasController(IReservaService reservaService)
        {
            _reservaService = reservaService;
        }
        public async Task<IActionResult> Index()
        {
            // 1. Obtiene el ID del usuario actual
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 2. Llama al servicio para obtener sus reservas
            var misReservas = await _reservaService.GetMisReservas(usuarioId);

            // 3. Envía la lista a la vista
            return View(misReservas);
        }

        [HttpGet]
        public async Task<IActionResult> ReservarEquipo()
        {
            var modelo = await _reservaService.GetDatosParaReservarEquipo();
            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReservarEquipo(ReservarEquipoModel model)
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!ModelState.IsValid)
            {
                // Recargar dropdown si falla
                var modeloRecargado = await _reservaService.GetDatosParaReservarEquipo();
                model.SalasDisponibles = modeloRecargado.SalasDisponibles;
                return View(model);
            }

            try
            {
                await _reservaService.CrearReservaEquipo(model, usuarioId);
                return RedirectToAction("Index"); // Vuelve a "Mis Reservas"
            }
            catch (InvalidOperationException ex) // Atrapa los errores de reglas de negocio
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                // Recargar dropdown
                var modeloRecargado = await _reservaService.GetDatosParaReservarEquipo();
                model.SalasDisponibles = modeloRecargado.SalasDisponibles;
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Cancelar(Guid id)
        {
            var reservaParaCancelar = new ReservaIndexModel { Id = id, ObjetoReservado = "Reserva N° " + id.ToString().Substring(0, 8) };
            return View(reservaParaCancelar);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarConfirmado(Guid id)
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            try
            {
                // Llama al servicio para ejecutar la lógica de cancelación
                await _reservaService.CancelarReserva(id, usuarioId);
                return RedirectToAction("Index");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(); // Error 403 - No tiene permiso
            }
            catch (InvalidOperationException ex)
            {
                // Si la reserva está en uso o finalizada
                var modelo = new ReservaIndexModel { Id = id, ObjetoReservado = "Reserva con conflicto" };
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("Cancelar", modelo);
            }
        }
    }
}
