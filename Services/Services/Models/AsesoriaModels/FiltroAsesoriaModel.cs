using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Models.AsesoriaModels
{
    public class FiltroAsesoriaModel
    {
        public string? Busqueda { get; set; }
        public EstadoAsesoria? Estado { get; set; }
        public DateTime? Fecha { get; set; }
        public int Pagina { get; set; } = 1;
    }
}
