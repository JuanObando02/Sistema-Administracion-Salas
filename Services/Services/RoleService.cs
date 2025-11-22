using Microsoft.AspNetCore.Identity;
using Services.Models.RolModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class RoleService : IRoleService
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleService(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<IList<IdentityRole>> GetRoles()
        {
            return await _roleManager.Roles.ToListAsync();
        }

        public async Task<IdentityResult> CrearRol(RolModel model)
        {
            // Crea un nuevo IdentityRole y lo guarda
            var rol = new IdentityRole(model.Nombre);
            return await _roleManager.CreateAsync(rol);
        }
    }
}
