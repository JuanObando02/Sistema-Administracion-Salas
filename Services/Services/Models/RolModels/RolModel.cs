using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Models.RolModels
{
    public class RolModel
    {
        [Required(ErrorMessage = "El nombre del rol es obligatorio")]
        [Display(Name = "Nombre del Rol")]
        public string Nombre { get; set; } = string.Empty;
    }
}
