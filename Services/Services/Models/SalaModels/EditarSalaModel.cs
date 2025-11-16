using Domain;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Models.SalaModels
{
    public enum EstadoSalaModel
    {
        Disponible,
        Ocupada,
        EnMantenimiento,
        Deshabilitada
    }
    public enum TipoSala
    {
        Individual, // Para préstamo de equipos uno por uno
        Clase_Completa // Para reserva de profesor
    }
    public class EditarSalaModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El número es obligatorio")]
        public int Numero { get; set; }

        [Required(ErrorMessage = "La capacidad es obligatoria")]
        [Range(1, 100)]
        public int Capacidad { get; set; }

        [Required]
        public EstadoSala Estado { get; set; } // El enum de Estado

        [Required(ErrorMessage = "El tipo de sala es obligatorio")]
        [Display(Name = "Tipo de Sala")]
        public TipoSala Tipo { get; set; } // El enum de Tipo
    }
}
