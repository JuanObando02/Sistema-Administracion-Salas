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
    /// <summary>
    /// Representa una solicitud de asesoría técnica de un usuario
    /// a un coordinador.
    /// </summary>
    public class Asesoria
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(500)]
        public string Descripcion { get; set; } = string.Empty; // La descripción del problema

        [Required]
        public EstadoAsesoria Estado { get; set; }

        [Required]
        public DateTime FechaSolicitud { get; set; }

        // --- Relación con Usuario (Quién la solicita) ---
        [Required]
        public string UsuarioId { get; set; } = string.Empty; // FK a Usuario 

        [ForeignKey("UsuarioId")]
        [InverseProperty("AsesoriasSolicitadas")]
        public AppUser UsuarioSolicitante { get; set; } = null!;

        // --- Relación con Coordinador (Quién la atiende) ---
        // Es "nullable" (Guid?) porque al inicio no tiene a nadie asignado.
        public string? CoordinadorId { get; set; } // FK a Usuario (Guid?)

        [ForeignKey("CoordinadorId")]
        [InverseProperty("AsesoriasAtendidas")]
        public AppUser CoordinadorAsignado { get; set; } = null!;

        public Asesoria()
        {
            Id = Guid.NewGuid();
            Estado = EstadoAsesoria.Pendiente;
            FechaSolicitud = DateTime.Now;
        }
    }
}
