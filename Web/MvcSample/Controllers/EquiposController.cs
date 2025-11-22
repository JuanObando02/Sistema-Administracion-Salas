using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.Models.EquipoModels;

namespace MvcSample.Controllers
{
    [Authorize(Roles = "Admin, Master, Coordinador")]
    public class EquiposController : Controller
    {
        private readonly IEquipoService _equipoService;
        public EquiposController(IEquipoService equipoService)
        {
            _equipoService = equipoService;

        }

        // --- ACCIÓN 1: Mostrar el formulario (GET) ---
        [HttpGet]
        public async Task<IActionResult> Registrar(Guid? salaId) // Acepta el ID de la sala
        {
            // Llama al servicio modificado
            var modelo = await _equipoService.GetDatosParaRegistrar(salaId);
            return View(modelo);
        }

        // --- ACCIÓN 2: Recibir los datos (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(RegistrarEquipoModel model)
        {
            if (!ModelState.IsValid)
            {
                var modeloRecargado = await _equipoService.GetDatosParaRegistrar(model.SalaId == Guid.Empty ? null : model.SalaId);
                modeloRecargado.Serial = model.Serial;
                return View(modeloRecargado);
            }

            try
            {
                var salaId = await _equipoService.RegistrarEquipo(model);
                return RedirectToAction("Index", new { salaId = salaId });
            }
            catch (InvalidOperationException ex)
            {
                // Atrapa el error específico de "sala llena"
                ModelState.AddModelError(string.Empty, ex.Message);

                // Recargamos el dropdown y devolvemos la vista con el error
                var modeloRecargado = await _equipoService.GetDatosParaRegistrar(model.SalaId == Guid.Empty ? null : model.SalaId);
                modeloRecargado.Serial = model.Serial;
                modeloRecargado.SalaId = model.SalaId;
                return View(modeloRecargado);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ocurrió un error al guardar el equipo.");
                var modeloRecargado = await _equipoService.GetDatosParaRegistrar(model.SalaId == Guid.Empty ? null : model.SalaId);
                return View(modeloRecargado);
            }

        }

        [HttpGet]
        public async Task<IActionResult> Index(Guid? salaId, string? buscarSerial)
        {
            ViewBag.SalaId = salaId;
            ViewData["FiltroSerial"] = buscarSerial;
            IList<EquipoIndexModel> listaEquipos;

            if (salaId.HasValue)
            {
                // Si el ID viene, filtra la lista
                listaEquipos = await _equipoService.GetEquiposPorSala(salaId.Value, buscarSerial);
            }
            else
            {
                // Si no, trae todos
                listaEquipos = await _equipoService.GetEquipos(buscarSerial);
            }

            return View(listaEquipos);
        }

        [HttpGet]// GET: /Equipos/Editar/guid
        public async Task<IActionResult> Editar(Guid id)
        {
            var modelo = await _equipoService.GetEquipoParaEditar(id);
            if (modelo == null) return NotFound();
            return View(modelo);
        }

        [HttpPost]// POST: /Equipos/Editar/
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(EditarEquipoModel model)
        {
            if (!ModelState.IsValid)
            {
                model = await _equipoService.RepopularDropdownsParaEditar(model);
                return View(model);
            }

            try
            {
                var salaId = await _equipoService.UpdateEquipo(model);
                return RedirectToAction("Index", new { salaId = salaId });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                // También repopulamos aquí si hay un error de lógica
                model = await _equipoService.RepopularDropdownsParaEditar(model);
                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Error al actualizar.");
                model = await _equipoService.RepopularDropdownsParaEditar(model);
                return View(model);
            }
        }
        
        [HttpGet]// GET: /Equipos/Eliminar/guid
        public async Task<IActionResult> Eliminar(Guid id)
        {
            var modelo = await _equipoService.GetEquipoParaEditar(id);
            if (modelo == null) return NotFound();
            return View(modelo); // Reutilizamos el EditarEquipoModel para mostrar datos
        }
        [HttpPost]// POST: /Equipos/EliminarConfirmado/guid
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(Guid id)
        {
            try
            {
                await _equipoService.DeleteEquipo(id);
                return RedirectToAction("Index");
            }
            catch (InvalidOperationException ex) // Error "Equipo en uso"
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var modelo = await _equipoService.GetEquipoParaEditar(id);
                return View("Eliminar", modelo);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Error al eliminar.");
                var modelo = await _equipoService.GetEquipoParaEditar(id);
                return View("Eliminar", modelo);
            }
        }
    }
}
