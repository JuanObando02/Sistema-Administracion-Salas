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

        // GET: /Usuario/Index
        public async Task<IActionResult> Index()
        {
            var usuarios = await _usuarioService.GetUsuarios();
            return View(usuarios);
        }

        // GET: /Usuario/Registrar
        public async Task<IActionResult> Registrar()
        {
            var modelo = await _usuarioService.GetDatosParaRegistrar();
            return View(modelo);
        }

        // POST: /Usuario/Registrar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(RegistrarUsuarioModel model)
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
    }
}
