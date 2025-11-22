using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums;

namespace Services.Models.EquipoModels
{
    public class EquipoIndexModel
    {
        public Guid Id { get; set; }
        public string Serial { get; set; } = string.Empty;
        public EstadoEquipo Estado { get; set; }
        public string SalaNombre { get; set; } = string.Empty; // Para "Sala 101"
    }
}
