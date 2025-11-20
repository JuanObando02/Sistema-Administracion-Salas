using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Domain.Enums;

namespace Services.Models.EquipoModels
{
    public class RegistrarEquipoModel
    {
        [Required(ErrorMessage = "El número de serial es obligatorio")]
        public string Serial { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar una sala")]
        [Display(Name = "Sala")]
        public Guid SalaId { get; set; }

        // Esta propiedad es para llenar el Dropdown en la vista
        public IEnumerable<SelectListItem> SalasDisponibles { get; set; } = new List<SelectListItem>();
    }
}
