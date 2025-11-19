using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Models.ReservaModels
{
    public class ReservarEquipoModel
    {
        [Required(ErrorMessage = "Debe seleccionar una sala.")]
        [Display(Name = "Sala")]
        public Guid SalaId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una fecha y hora de inicio.")]
        [Display(Name = "Fecha y Hora de Inicio")]
        public DateTime FechaInicio { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Debe seleccionar la duración.")]
        [Range(1, 3, ErrorMessage = "No puede reservar más de 3 horas.")]
        [Display(Name = "Duración (en horas)")]
        public int DuracionHoras { get; set; } = 1;

        // Para el dropdown
        public IEnumerable<SelectListItem> SalasDisponibles { get; set; } = new List<SelectListItem>();
    }
}
