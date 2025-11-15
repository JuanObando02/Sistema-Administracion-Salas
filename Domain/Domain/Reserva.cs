using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums;

namespace Domain
{
    public class Reserva
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public TipoReserva Tipo { get; set; }

        [Required]
        public DateTime FechaInicio { get; set; }
        [Required]
        public DateTime FechaFin { get; set; }

        [Required]
        public EstadoReserva Estado { get; set; }

        // --- Relación con Usuario (Quién lo pide) ---
        [Required]
        public string UsuarioId { get; set; } = string.Empty; 

        [ForeignKey("UsuarioId")]
        [InverseProperty("ReservasHechas")]
        public AppUser UsuarioSolicitante { get; set; } = null!;

        // --- Relación con Coordinador ---
        public string? AprobadorId { get; set; } // FK a AppUser

        [ForeignKey("AprobadorId")]
        [InverseProperty("ReservasAprobadas")]
        public AppUser UsuarioAprobador { get; set; } = null!;

        // --- Relación Sala O Equipo ---
        // Deben ser Nullable y de tipo Guid?
        public Guid? SalaId { get; set; } // nulo por si es reserva de equipo
        [ForeignKey("SalaId")]
        public Sala Sala { get; set; } = null!;

        public Guid? EquipoId { get; set; } //nulo por si es reserva de sala
        [ForeignKey("EquipoId")]
        public Equipo Equipo { get; set; } = null!;

        public Reserva()
        {
            Id = Guid.NewGuid();
            Estado = EstadoReserva.Pendiente;
        }
    }
}
