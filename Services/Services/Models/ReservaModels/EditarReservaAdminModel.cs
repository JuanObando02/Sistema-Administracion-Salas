using Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Models.ReservaModels
{
    public class EditarReservaAdminModel
    {
        public Guid Id { get; set; }

        public TipoReserva Tipo { get; set; } // Solo lectura (no cambiamos de Sala a Equipo)

        [Display(Name = "Fecha Inicio")]
        public DateTime FechaInicio { get; set; }

        [Display(Name = "Fecha Fin")]
        public DateTime FechaFin { get; set; }

        [Display(Name = "Sala Asignada")]
        public Guid? SalaId { get; set; }

        [Display(Name = "Equipo Asignado")]
        public Guid? EquipoId { get; set; }

        // Dropdowns
        public IEnumerable<SelectListItem> SalasDisponibles { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> EquiposDisponibles { get; set; } = new List<SelectListItem>();
    }
}
