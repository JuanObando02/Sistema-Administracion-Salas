using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Models.ReporteModels
{
    public class FiltroReporteModel
    {
        public string? Busqueda { get; set; }
        public EstadoReporte? Estado { get; set; }
        public DateTime? Fecha { get; set; }
        public int Pagina { get; set; } = 1;
    }
}
