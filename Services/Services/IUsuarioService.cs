using Microsoft.AspNetCore.Identity;
using Services.Models.UsuarioModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IUsuarioService
    {
        Task<IList<UsuarioIndexModel>> GetUsuarios();
        Task<RegistrarUsuarioModel> GetDatosParaRegistrar();
        Task<IdentityResult> RegistrarUsuario(RegistrarUsuarioModel model);
    }
}
