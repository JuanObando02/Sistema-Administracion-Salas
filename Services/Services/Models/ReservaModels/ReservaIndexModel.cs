using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Models.ReservaModels
{
    public class ReservaIndexModel
    {
        public Guid Id { get; set; }
        public TipoReserva Tipo { get; set; }
        public string ObjetoReservado { get; set; } = string.Empty; // "Sala 2410" o "Serial A000B1"
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public EstadoReserva Estado { get; set; }
        public Guid? SalaId { get; set; }
        public Guid? EquipoId { get; set; }
        public string NombreSolicitante { get; set; } = string.Empty;
        public string EmailSolicitante { get; set; } = string.Empty;
        public string DocumentoSolicitante { get; set; } = string.Empty;
    }
}
