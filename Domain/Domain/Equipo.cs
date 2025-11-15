using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums;
namespace Domain
{
    public class Equipo
    {
        [Key]
        public Guid Id { get; set; }
      
        [Required]
        public string Serial { get; set; } = string.Empty;
        [Required]
        public EstadoEquipo Estado { get; set; }
        // Llave foránea a Sala
        [Required]
        public Guid SalaId { get; set; }
        public Sala Sala { get; set; } = null!;

        // --- Listas de Navegación ---
        public List<Reserva> Reservas { get; set; }
        public List<Reporte> Reportes { get; set; }

        public Equipo()
        {
            Reservas = new List<Reserva>();
            Reportes = new List<Reporte>();
            Estado = EstadoEquipo.Disponible;
        }

    }
}
