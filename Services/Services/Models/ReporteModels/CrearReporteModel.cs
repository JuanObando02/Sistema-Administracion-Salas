using Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Models.ReporteModels
{
    public class CrearReporteModel
    {
        [Required(ErrorMessage = "Seleccione el tipo de daño.")]
        [Display(Name = "Tipo de Incidente")]
        public TipoReporte Tipo { get; set; } // Sala o Equipo

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(500, ErrorMessage = "Máximo 500 caracteres.")]
        public string Descripcion { get; set; } = string.Empty;

        [Display(Name = "Sala Afectada")]
        public Guid? SalaId { get; set; } // Opcional si es un equipo (pero útil para filtrar)

        [Display(Name = "Equipo Afectado")]
        public Guid? EquipoId { get; set; } // Opcional si es daño de sala

        // Listas para los dropdowns
        public IEnumerable<SelectListItem> SalasDisponibles { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> EquiposDisponibles { get; set; } = new List<SelectListItem>();
    }
}
