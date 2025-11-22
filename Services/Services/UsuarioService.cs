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

        public async Task<EditarUsuarioRolesModel> GetUsuarioParaEditarRoles(string id)
        {
            // 1. Buscar usuario
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null) return null;

            // 2. Obtener roles actuales del usuario
            var rolesUsuario = await _userManager.GetRolesAsync(usuario);

            // 3. Obtener TODOS los roles del sistema
            var todosLosRoles = await _roleManager.Roles.ToListAsync();

            // 4. Construir el modelo
            var model = new EditarUsuarioRolesModel
            {
                UsuarioId = usuario.Id,
                Email = usuario.Email,
                Roles = todosLosRoles.Select(r => new RoleSelection
                {
                    RoleName = r.Name,
                    // Marcamos como true si el usuario YA tiene ese rol
                    IsSelected = rolesUsuario.Contains(r.Name)
                }).ToList()
            };

            return model;
        }

        public async Task ActualizarRolesUsuario(EditarUsuarioRolesModel model)
        {
            var usuario = await _userManager.FindByIdAsync(model.UsuarioId);
            if (usuario == null) throw new Exception("Usuario no encontrado.");

            // 1. Obtener roles actuales
            var rolesActuales = await _userManager.GetRolesAsync(usuario);

            // 2. Obtener roles seleccionados en el formulario
            var rolesSeleccionados = model.Roles.Where(r => r.IsSelected).Select(r => r.RoleName).ToList();

            // 3. Calcular cuáles añadir (Seleccionados que NO tiene actualmente)
            var rolesAAnadir = rolesSeleccionados.Except(rolesActuales);

            // 4. Calcular cuáles quitar (Actuales que NO están seleccionados)
            var rolesAQuitar = rolesActuales.Except(rolesSeleccionados);

            // 5. Ejecutar cambios
            await _userManager.AddToRolesAsync(usuario, rolesAAnadir);
            await _userManager.RemoveFromRolesAsync(usuario, rolesAQuitar);
        }

        public async Task EliminarUsuario(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null) throw new Exception("Usuario no encontrado.");

            // Intenta eliminar
            var resultado = await _userManager.DeleteAsync(usuario);

            if (!resultado.Succeeded)
            {
                // Si falla (probablemente por la restricción de llaves foráneas en la BD)
                throw new InvalidOperationException("No se puede eliminar el usuario. Es probable que tenga reservas, reportes o asesorías asociadas.");
            }
        }
        public async Task<IEnumerable<SelectListItem>> GetUsuariosParaDropdown()
        {
            var usuarios = await _userManager.Users.ToListAsync();

            return usuarios.Select(u => new SelectListItem
            {
                Value = u.Id,
                Text = $"{u.Name} {u.LastName} (Doc: {u.DocumentNumber})"
            }).OrderBy(u => u.Text);
        }
    }
}
