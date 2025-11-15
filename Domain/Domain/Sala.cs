using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums;

namespace Domain
{
    public class Sala
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public int Numero { get; set; }

        [Required]
        public EstadoSala Estado { get; set; }

        [Required]
        public int Capacidad { get; set; }

        public List<Equipo> Equipos { get; set; }
        public List<Reporte> Reportes { get; set; }
        public List<Reserva> Reservas { get; set; }

        public Sala()
        {
            Equipos = new List<Equipo>();
            Reportes = new List<Reporte>();
            Reservas = new List<Reserva>();
            Estado = EstadoSala.Disponible;
        }


    }
}
