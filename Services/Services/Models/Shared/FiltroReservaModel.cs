using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Models.ReservaModels
{
    public class FiltroReservaModel
    {
        public string? Busqueda { get; set; } // Serial o Número Sala
        public TipoReserva? Tipo { get; set; } // Filtro Sala vs Equipo
        public DateTime? Fecha { get; set; }   // Filtro Fecha
        public string OrdenarPor { get; set; } = "fecha_desc"; // Ordenamiento
        public int Pagina { get; set; } = 1;
        public int RegistrosPorPagina { get; set; } = 10;
    }
}
