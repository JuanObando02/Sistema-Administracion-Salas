using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.Models.UsuarioModels;

namespace MvcSample.Controllers
{
    [Authorize(Roles = "Admin, Master")]
    public class UsuariosController : Controller
    {
        private readonly IUsuarioService _usuarioService;

        public UsuariosController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }
        [HttpGet]
        public async Task<IActionResult> Index() //vista usuarios
        {
            var usuarios = await _usuarioService.GetUsuarios();
            return View(usuarios);
        }

        [HttpGet]
        public async Task<IActionResult> Registrar() //vista registrar usuario
        {
            var modelo = await _usuarioService.GetDatosParaRegistrar();
            return View(modelo);
        }

        // POST: /Usuario/Registrar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(RegistrarUsuarioModel model) //registrar usuario
        {
            if (!ModelState.IsValid)
            {
                // Si falla, recarga el dropdown
                var modeloRecargado = await _usuarioService.GetDatosParaRegistrar();
                model.RolesDisponibles = modeloRecargado.RolesDisponibles;
                return View(model);
            }

            var resultado = await _usuarioService.RegistrarUsuario(model);

            if (resultado.Succeeded)
            {
                return RedirectToAction("Index");
            }

            // Si falla (ej. email ya existe, contraseña débil), muestra errores
            foreach (var error in resultado.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            // Recarga el dropdown
            var modeloRecargadoError = await _usuarioService.GetDatosParaRegistrar();
            model.RolesDisponibles = modeloRecargadoError.RolesDisponibles;
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Editar(string id)
        {
            var model = await _usuarioService.GetUsuarioParaEditarRoles(id);
            if (model == null) return NotFound();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(EditarUsuarioRolesModel model)
        {
            try
            {
                await _usuarioService.ActualizarRolesUsuario(model);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error al actualizar roles.");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Eliminar(string id)
        {
            var model = await _usuarioService.GetUsuarioParaEditarRoles(id);
            if (model == null) return NotFound();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(string UsuarioId)
        {
            try
            {
                await _usuarioService.EliminarUsuario(UsuarioId);
                return RedirectToAction("Index");
            }
            catch (InvalidOperationException ex)
            {
                // Error de restricción (tiene reservas)
                ModelState.AddModelError(string.Empty, ex.Message);
                var model = await _usuarioService.GetUsuarioParaEditarRoles(UsuarioId);
                return View("Eliminar", model);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Error inesperado al eliminar.");
                var model = await _usuarioService.GetUsuarioParaEditarRoles(UsuarioId);
                return View("Eliminar", model);
            }
        }
    }
}
