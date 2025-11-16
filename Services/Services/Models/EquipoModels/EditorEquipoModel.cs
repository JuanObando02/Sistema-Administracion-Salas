using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Models.EquipoModels
{
    public class EditarEquipoModel
    {
        public Guid Id { get; set; }

        [Required]
        public string Serial { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Sala")]
        public Guid SalaId { get; set; }

        [Required]
        public EstadoEquipoModel Estado { get; set; }

        // Para el dropdown de Salas
        public IEnumerable<SelectListItem> SalasDisponibles { get; set; } = new List<SelectListItem>();
    }
}
