using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Models.ReservaModels
{
    public class ReservarEquipoCoordinadorModel : ReservarEquipoModel
    {
        [Required(ErrorMessage = "Debe seleccionar el usuario beneficiario.")]
        [Display(Name = "Reservar para el Usuario")]
        public string UsuarioIdSeleccionado { get; set; } = string.Empty;

        public IEnumerable<SelectListItem> UsuariosDisponibles { get; set; } = new List<SelectListItem>();
    }
}
