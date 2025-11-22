using Microsoft.AspNetCore.Identity;
using Services.Models.RolModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IRoleService
    {
        Task<IList<IdentityRole>> GetRoles();
        Task<IdentityResult> CrearRol(RolModel model);
    }
}
