using Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Models.SalaModels
{
    public class RegistrarSalaModel
    {
        [Required(ErrorMessage = "El número es obligatorio")]
        [Display(Name = "Número de Sala")]
        public int Numero { get; set; }

        [Required(ErrorMessage = "La capacidad es obligatoria")]
        [Range(1, 100, ErrorMessage = "La capacidad debe ser al menos 1")]
        public int Capacidad { get; set; }
    }
}
