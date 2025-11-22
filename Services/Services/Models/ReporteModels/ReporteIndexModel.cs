using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Models.ReporteModels
{
    public class ReporteIndexModel
    {
        public Guid Id { get; set; }
        public TipoReporte Tipo { get; set; }
        public string ObjetoAfectado { get; set; } = string.Empty; // "Sala 101" o "Equipo A01"
        public string Descripcion { get; set; } = string.Empty;
        public EstadoReporte Estado { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
