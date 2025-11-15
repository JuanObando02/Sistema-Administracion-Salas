using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Services.Models.SalaModels
{
    public class SalaIndexModel
    {
        public Guid Id { get; set; }
        public int Numero { get; set; } // Lo usaremos para "Sala [Numero]"
        public int Capacidad { get; set; }
        public EstadoSalaModel Estado { get; set; }
    }
}
