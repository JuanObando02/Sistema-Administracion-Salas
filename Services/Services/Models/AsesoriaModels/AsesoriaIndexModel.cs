using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Models.AsesoriaModels
{
    public class AsesoriaIndexModel
    {
        public Guid Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string Ubicacion { get; set; } = string.Empty; // "Sala 101" o "Sin ubicación"
        public EstadoAsesoria Estado { get; set; }
    }
}
