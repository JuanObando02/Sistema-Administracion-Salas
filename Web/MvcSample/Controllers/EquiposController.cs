using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.Models.EquipoModels;

namespace MvcSample.Controllers
{
    [Authorize(Roles = "Admin, Master")]
    public class EquiposController : Controller
    {
        private readonly IEquipoService _equipoService;
        public EquiposController(IEquipoService equipoService)
        {
            _equipoService = equipoService;

        }

        // --- ACCIÓN 1: Mostrar el formulario (GET) ---
        [HttpGet]
        public async Task<IActionResult> Registrar()
        {
            // Llama al servicio para obtener el modelo CON la lista de salas
            var modelo = await _equipoService.GetDatosParaRegistrar();
            return View(modelo);
        }

        // --- ACCIÓN 2: Recibir los datos (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(RegistrarEquipoModel model)
        {
            if (!ModelState.IsValid)
            {
                // Si el modelo no es válido recargar la lista de salas antes de devolver la vista.
                var modeloRecargado = await _equipoService.GetDatosParaRegistrar();
                modeloRecargado.Serial = model.Serial; // Mantiene el serial que el usuario escribió
                return View(modeloRecargado);
            }

            try
            {
                await _equipoService.RegistrarEquipo(model);
                return RedirectToAction("Index", "Equipos"); // Vuelve al Index de Salas
            }
            catch (InvalidOperationException ex)
            {
                // Atrapa el error específico de "sala llena"
                ModelState.AddModelError(string.Empty, ex.Message);

                // Recargamos el dropdown y devolvemos la vista con el error
                var modeloRecargado = await _equipoService.GetDatosParaRegistrar();
                modeloRecargado.Serial = model.Serial;
                modeloRecargado.SalaId = model.SalaId;
                return View(modeloRecargado);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ocurrió un error al guardar el equipo.");
                var modeloRecargado = await _equipoService.GetDatosParaRegistrar();
                return View(modeloRecargado);
            }

        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var listaEquipos = await _equipoService.GetEquipos();
            return View(listaEquipos);
        }

        // GET: /Equipos/Editar/guid
        [HttpGet]
        public async Task<IActionResult> Editar(Guid id)
        {
            var modelo = await _equipoService.GetEquipoParaEditar(id);
            if (modelo == null) return NotFound();
            return View(modelo);
        }

        // POST: /Equipos/Editar/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(EditarEquipoModel model)
        {
            if (!ModelState.IsValid)
            {
                // --- ¡ARREGLO AQUÍ! ---
                // Llama al nuevo método del servicio para repopular
                model = await _equipoService.RepopularDropdownsParaEditar(model);
                return View(model);
            }

            try
            {
                await _equipoService.UpdateEquipo(model);
                return RedirectToAction("Index");
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

        // GET: /Equipos/Eliminar/guid
        [HttpGet]
        public async Task<IActionResult> Eliminar(Guid id)
        {
            var modelo = await _equipoService.GetEquipoParaEditar(id);
            if (modelo == null) return NotFound();
            return View(modelo); // Reutilizamos el EditarEquipoModel para mostrar datos
        }

        // POST: /Equipos/EliminarConfirmado/guid
        [HttpPost]
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
