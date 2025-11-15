using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums;

namespace Domain
{

    public class Reporte
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public TipoReporte Tipo { get; set; }

        [Required]
        [StringLength(500)]
        public string Descripcion { get; set; } = string.Empty;

        [Required]
        public EstadoReporte Estado { get; set; }

        [Required]
        public DateTime FechaCreacion { get; set; }

        // --- Relación con Usuario ---
        [Required]
        public string UsuarioId { get; set; } = string.Empty; // La llave foránea al usuario

        [ForeignKey("UsuarioId")]
        [InverseProperty("ReportesCreados")] // Conecta a la lista en Usuario.cs
        public AppUser UsuarioCreador { get; set; } = null!;

        // --- Relación con Sala O Equipo ---
        // Un reporte es O de una sala O de un equipo, no de ambos.
        // Usamos llaves foráneas "nullable" (Guid?) para manejar esto.

        public Guid? SalaId { get; set; } // Nullable
        [ForeignKey("SalaId")]
        public Sala SalaReportada { get; set; } = null!;

        public Guid? EquipoId { get; set; } // Nullable
        [ForeignKey("EquipoId")]
        public Equipo EquipoReportado { get; set; } = null!;

        public Reporte()
        {
            Estado = EstadoReporte.Pendiente;
            FechaCreacion = DateTime.Now;
        }

    }
}
