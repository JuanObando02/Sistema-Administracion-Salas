using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.Models.ReservaModels;
using System.Security.Claims;

namespace MvcSample.Controllers
{
    [Authorize(Roles = "Admin, Master, Estudiante, Profesor, Coordinador")]
    public class ReservasController : Controller
    {
        private readonly IReservaService _reservaService;
        private readonly IEquipoService _equipoService;

        public ReservasController(IReservaService reservaService, IEquipoService equipoService)
        {
            _reservaService = reservaService;
            _equipoService = equipoService;
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
            bool esProfesor = User.IsInRole("Profesor");

            if (!ModelState.IsValid)
            {
                // Recargar dropdown si falla
                var modeloRecargado = await _reservaService.GetDatosParaReservarEquipo();
                model.SalasDisponibles = modeloRecargado.SalasDisponibles;
                return View(model);
            }

            try
            {
                await _reservaService.CrearReservaEquipo(model, usuarioId, esProfesor);
                return RedirectToAction("Index"); // Vuelve a "Mis Reservas"
            }
            catch (InvalidOperationException ex) // Atrapa los errores de reglas de negocio
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                // Recargar dropdown
                var datosFrescos = await _reservaService.GetDatosParaReservarEquipo();
                model.SalasDisponibles = datosFrescos.SalasDisponibles;

                return View(model);
            }
            catch (Exception) // Errores inesperados
            {
                ModelState.AddModelError(string.Empty, "Ocurrió un error inesperado al reservar.");

                var datosFrescos = await _reservaService.GetDatosParaReservarEquipo();
                model.SalasDisponibles = datosFrescos.SalasDisponibles;

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
        
        [HttpGet]
        [Authorize(Roles = "Profesor")] // Solo profesores
        public async Task<IActionResult> ReservarSala()
        {
            var model = await _reservaService.GetDatosParaReservarSala();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Profesor")]
        public async Task<IActionResult> ReservarSala(ReservarSalaModel model)
        {
            if (!ModelState.IsValid)
            {
                var datosFrescos = await _reservaService.GetDatosParaReservarSala();
                model.SalasDisponibles = datosFrescos.SalasDisponibles;
                return View(model);
            }

            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            try
            {
                await _reservaService.CrearReservaSala(model, usuarioId);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var datosFrescos = await _reservaService.GetDatosParaReservarSala();
                model.SalasDisponibles = datosFrescos.SalasDisponibles;
                return View(model);
            }
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Finalizar(Guid id)
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            try
            {
                await _reservaService.FinalizarReserva(id, usuarioId);
                TempData["Mensaje"] = "Reserva finalizada y equipo liberado exitosamente.";
            }
            catch (Exception ex)
            {
                // Usamos TempData para mostrar el error en la vista Index sin romper el flujo
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize(Roles = "Coordinador, Admin")]
        public async Task<IActionResult> Gestionar(string? busqueda,TipoReserva? tipo,DateTime? fecha,string ordenarPor = "fecha_desc",int pagina = 1)
        {
            // Creamos el objeto de filtro
            var filtro = new FiltroReservaModel
            {
                Busqueda = busqueda,
                Tipo = tipo,
                Fecha = fecha,
                OrdenarPor = ordenarPor,
                Pagina = pagina,
                RegistrosPorPagina = 10 // Puedes cambiar esto
            };

            var listaPaginada = await _reservaService.GetReservasGestionar(filtro);

            // Pasamos los filtros actuales a la vista para mantener el estado de los inputs
            ViewData["BusquedaActual"] = busqueda;
            ViewData["TipoActual"] = tipo;
            ViewData["FechaActual"] = fecha?.ToString("yyyy-MM-dd");
            ViewData["OrdenActual"] = ordenarPor;

            return View(listaPaginada);
        }

        [HttpPost]
        [Authorize(Roles = "Coordinador, Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aprobar(Guid id)
        {
            var coordinadorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            try
            {
                await _reservaService.AprobarReserva(id, coordinadorId);
                TempData["Mensaje"] = "Reserva aprobada exitosamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Gestionar"); 
        }

        [HttpPost]
        [Authorize(Roles = "Coordinador, Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rechazar(Guid id)
        {
            var coordinadorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            try
            {
                await _reservaService.RechazarReserva(id, coordinadorId);
                TempData["Mensaje"] = "Reserva denegada.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Gestionar");
        }

        [HttpGet]
        [Authorize(Roles = "Coordinador, Admin")]
        public async Task<IActionResult> EditarAdmin(Guid id)
        {
            var model = await _reservaService.GetReservaParaEditarAdmin(id);
            if (model == null) return NotFound();
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Coordinador, Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarAdmin(EditarReservaAdminModel model)
        {
            // Si el formulario está incompleto (Validación automática)
            if (!ModelState.IsValid)
            {
                // Debemos recargar las listas antes de devolver la vista
                model = await _reservaService.RepopularDropdownsEditarAdmin(model);
                // ------------------------------
                return View(model);
            }

            var coordinadorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            try
            {
                await _reservaService.ActualizarReservaAdmin(model, coordinadorId);
                TempData["Mensaje"] = "Reserva actualizada correctamente.";
                return RedirectToAction("Gestionar");
            }
            catch (Exception ex)
            {
                // Muestra el error en la pantalla
                ModelState.AddModelError("", ex.Message);

                model = await _reservaService.RepopularDropdownsEditarAdmin(model);
                // ------------------------------

                return View(model);
            }
        }
        
        [HttpPost]
        [Authorize(Roles = "Coordinador, Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarAdmin(Guid id)
        {
            try
            {
                await _reservaService.EliminarReservaAdmin(id);
                TempData["Mensaje"] = "Reserva eliminada.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar: " + ex.Message;
            }
            return RedirectToAction("Gestionar");
        }

        [HttpGet]
        public async Task<IActionResult> GetEquiposPorSalaJson(Guid salaId, DateTime? inicio, DateTime? fin, Guid? reservaId)
        {
            // Validar que tengamos fechas. Si no, no podemos filtrar por horario,
            if (inicio == null || fin == null)
            {
                // Opción A: Devolver todos si no hay fecha seleccionada
                var todos = await _equipoService.GetEquiposPorSala(salaId);
                return Json(todos.Select(e => new { value = e.Id, text = e.Serial }));
            }

            // Llamar al servicio de filtrado
            var equiposDisponibles = await _equipoService.GetEquiposDisponibles(salaId, inicio.Value, fin.Value, reservaId);

            var listaParaDropdown = equiposDisponibles.Select(e => new
            {
                value = e.Id,
                text = e.Serial
            });

            return Json(listaParaDropdown);
        }

        [HttpGet]
        [Authorize(Roles = "Coordinador, Admin")]
        public async Task<IActionResult> Pendientes()
        {
            var listaPendientes = await _reservaService.GetReservasPendientes();
            return View(listaPendientes);
        }
    }
}