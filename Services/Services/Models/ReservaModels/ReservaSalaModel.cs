using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Models.ReservaModels
{
    public class ReservarSalaModel
    {
        [Required(ErrorMessage = "Debe seleccionar una sala de clase.")]
        [Display(Name = "Sala de Clase")]
        public Guid SalaId { get; set; }

        [Required(ErrorMessage = "Fecha de inicio requerida.")]
        [Display(Name = "Fecha y Hora de Inicio")]
        public DateTime FechaInicio { get; set; } = DateTime.Now;

        [Required]
        [Range(1, 4, ErrorMessage = "La duración máxima es de 4 horas.")] // Profesores suelen tener clases de 1-4 horas
        public int DuracionHoras { get; set; } = 2;

        // Dropdown
        public IEnumerable<SelectListItem> SalasDisponibles { get; set; } = new List<SelectListItem>();
    }
}
