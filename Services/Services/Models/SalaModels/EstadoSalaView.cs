using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Models.SalaModels
{
    public class EstadoSalaViewModel
    {
        public Guid Id { get; set; }
        public string NombreSala { get; set; } = string.Empty;
        public int Capacidad { get; set; }
        public int EquiposDisponibles { get; set; }
        public EstadoSala Estado { get; set; }
        public TipoSala Tipo { get; set; }
    }
}
