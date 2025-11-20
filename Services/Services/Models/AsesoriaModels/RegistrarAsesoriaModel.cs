using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Models.AsesoriaModels
{
    public class RegistrarAsesoriaModel
    {
        [Required(ErrorMessage = "Describa en qué necesita ayuda.")]
        [StringLength(500)]
        [Display(Name = "Descripción de la Solicitud")]
        public string Descripcion { get; set; } = string.Empty;

        [Display(Name = "¿En qué sala se encuentra?")]
        public Guid? SalaId { get; set; }

        // Dropdown
        public IEnumerable<SelectListItem> SalasDisponibles { get; set; } = new List<SelectListItem>();
    }
}
