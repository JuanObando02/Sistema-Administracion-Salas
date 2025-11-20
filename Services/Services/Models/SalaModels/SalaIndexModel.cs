using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums;


namespace Services.Models.SalaModels
{
    public class SalaIndexModel
    {
        public Guid Id { get; set; }
        public int Numero { get; set; } // Lo usaremos para "Sala [Numero]"
        public int Capacidad { get; set; }
        public EstadoSala Estado { get; set; }
        public TipoSala Tipo { get; set; }
    }
}
