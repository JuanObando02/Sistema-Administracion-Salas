using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class AppUser:IdentityUser
    {
        public string Name { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        // --- Listas de Navegación ---
        [InverseProperty("UsuarioSolicitante")]
        public List<Reserva> ReservasHechas { get; set; }

        [InverseProperty("UsuarioAprobador")]
        public List<Reserva> ReservasAprobadas { get; set; }

        [InverseProperty("UsuarioCreador")]
        public List<Reporte> ReportesCreados { get; set; }

        // (Listas para la clase Asesoria)
        [InverseProperty("UsuarioSolicitante")]
        public List<Asesoria> AsesoriasSolicitadas { get; set; }

        [InverseProperty("CoordinadorAsignado")]
        public List<Asesoria> AsesoriasAtendidas { get; set; }

        public AppUser()
        {
            // Inicializamos las listas
            ReservasHechas = new List<Reserva>();
            ReservasAprobadas = new List<Reserva>();
            ReportesCreados = new List<Reporte>();
            AsesoriasSolicitadas = new List<Asesoria>();
            AsesoriasAtendidas = new List<Asesoria>();
        }
    }
}
