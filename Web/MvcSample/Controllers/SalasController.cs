using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.Models.SalaModels;
using System;

namespace MvcSample.Controllers
{
    [Authorize(Roles = "Admin, Master")]
    public class SalasController : Controller
    {
        private readonly ISalaService _salaService;

        // 1. Inyecta el servicio
        public SalasController(ISalaService salaService)
        {
            _salaService = salaService;
        }

        // 2. Acción GET para MOSTRAR el formulario de registro
        [HttpGet]
        public IActionResult Registrar()
        {
            // Simplemente devuelve la vista (debes crear Registrar.cshtml)
            return View();
        }

        // 3. Acción POST para RECIBIR los datos del formulario
        [HttpPost]
        [ValidateAntiForgeryToken] // Buena práctica de seguridad
        public async Task<IActionResult> Registrar(RegistrarSalaModel model)
        {
            // 4. Verifica si el modelo es válido (usando los [Required] del ViewModel)
            if (!ModelState.IsValid)
            {
                // Si no es válido, vuelve a mostrar el formulario con los errores
                return View(model);
            }

            try
            {
                // 5. Llama al servicio para hacer el trabajo
                await _salaService.RegistrarSala(model);

                // 6. Redirige a una página de éxito (ej. un Index de Salas)
                // (Debes crear una acción Index en este controlador)
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                // 7. Manejo básico de errores
                ModelState.AddModelError(string.Empty, "Ocurrió un error al guardar la sala.");
                return View(model);
            }
        }

        // --- ACCIÓN 3: Mostrar el formulario para EDITAR (HTTP GET) ---
        // Se activa con la URL: /Salas/Editar/un-guid-aqui
        [HttpGet]
        public async Task<IActionResult> Editar(Guid id)
        {
            // Llama al servicio para buscar la sala por su ID
            var model = await _salaService.GetSalaParaEditar(id);

            // Si el servicio devuelve null (porque no encontró el ID),
            // muestra un error 404
            if (model == null)
            {
                return NotFound();
            }

            // Si la encuentra, muestra la vista "Editar.cshtml"
            // y la rellena con los datos del 'model'
            return View(model);
        }

        // --- ACCIÓN 4: Recibir los datos del formulario de EDICIÓN (HTTP POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken] // Buena práctica de seguridad
        public async Task<IActionResult> Editar(EditarSalaModel model)
        {
            // 1. Verifica si los datos del formulario son válidos
            //    (Usando las anotaciones [Required], [Range] de tu ViewModel)
            if (!ModelState.IsValid)
            {
                // Si no, vuelve a mostrar el formulario con los mensajes de error
                return View(model);
            }

            try
            {
                // 2. Si son válidos, llama al servicio para actualizar
                await _salaService.UpdateSala(model);

                // 3. Redirige al listado de salas (Debes crear un Index)
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                // 4. Si algo falla al guardar, muestra un error general
                ModelState.AddModelError(string.Empty, "Ocurrió un error al actualizar la sala.");
                return View(model);
            }
        }

        // --- ACCIÓN 5: GET PARA MOSTRAR CONFIRMACIÓN ---
        // Se activa con: /Salas/Eliminar/guid-de-la-sala
        [HttpGet]
        public async Task<IActionResult> Eliminar(Guid id)
        {
            // Podemos re-usar el ViewModel de Editar para mostrar los datos
            var model = await _salaService.GetSalaParaEditar(id);
            if (model == null)
            {
                return NotFound();
            }
            return View(model); // Envía los datos a la vista "Eliminar.cshtml"
        }

        // --- ACCIÓN 6: POST PARA EJECUTAR LA ELIMINACIÓN ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Le damos un nombre de acción diferente para evitar conflictos
        public async Task<IActionResult> EliminarConfirmado(Guid id)
        {
            try
            {
                // Llama al servicio para que ejecute la lógica
                await _salaService.DeleteSala(id);
                return RedirectToAction("Index"); // Vuelve al listado
            }
            catch (InvalidOperationException ex)
            {
                // Recargamos el modelo para mostrar la vista de nuevo
                var model = await _salaService.GetSalaParaEditar(id);
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("Eliminar", model);
            }
            catch (Exception)
            {
                // Otro error inesperado
                var model = await _salaService.GetSalaParaEditar(id);
                ModelState.AddModelError(string.Empty, "Ocurrió un error inesperado al eliminar.");
                return View("Eliminar", model);
            }
        }
        
        // --- ACCIÓN 7: LISTAR TODAS LAS SALAS ---
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // 1. Llama al servicio para obtener la lista
            var listaSalas = await _salaService.GetSalas();

            // 2. Envía la lista a la Vista
            return View(listaSalas);
        }
    }
}
