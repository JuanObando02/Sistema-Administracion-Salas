using AutoMapper;
using Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Services.Models.UsuarioModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;

        public UsuarioService(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, IMapper mapper)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
        }

        public async Task<IList<UsuarioIndexModel>> GetUsuarios()
        {
            var usuarios = await _userManager.Users.ToListAsync();
            var listaModelo = new List<UsuarioIndexModel>();

            foreach (var usuario in usuarios)
            {
                var modelo = _mapper.Map<UsuarioIndexModel>(usuario);
                var roles = await _userManager.GetRolesAsync(usuario);
                modelo.Roles = string.Join(", ", roles); // "Admin, Coordinador"
                listaModelo.Add(modelo);
            }
            return listaModelo;
        }

        public async Task<RegistrarUsuarioModel> GetDatosParaRegistrar()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            var modelo = new RegistrarUsuarioModel
            {
                RolesDisponibles = roles.Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name
                })
            };
            return modelo;
        }

        public async Task<IdentityResult> RegistrarUsuario(RegistrarUsuarioModel model)
        {
            // 1. Mapea el modelo a la entidad AppUser
            var usuario = _mapper.Map<AppUser>(model);
            usuario.UserName = model.Email; // Identity usa UserName para el login

            // 2. Crea el usuario (esto hashea la contraseña)
            var resultado = await _userManager.CreateAsync(usuario, model.Password);

            if (resultado.Succeeded)
            {
                // 3. Si se crea, asigna el rol seleccionado
                await _userManager.AddToRoleAsync(usuario, model.RolSeleccionado);
            }

            return resultado;
        }
    }
}
