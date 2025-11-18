using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.Models.RolModels;

namespace MvcSample.Controllers
{
    [Authorize(Roles = "Admin, Master")]
    public class RolesController: Controller
    {
        private readonly IRoleService _roleService;
        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        public async Task<IActionResult> Index()
        {
            var roles = await _roleService.GetRoles();
            return View(roles);
        }

        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(RolModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var resultado = await _roleService.CrearRol(model);

            if (resultado.Succeeded)
            {
                return RedirectToAction("Index");
            }

            foreach (var error in resultado.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }
    }
}
