using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums
{
    public enum EstadoAsesoria
    {
        Pendiente,      // El usuario la acaba de enviar
        EnProceso,      // Un coordinador ya la está atendiendo
        Cerrada,        // El problema fue resuelto
        NoAplica        // El coordinador la cierra sin acción
    }
    public enum EstadoEquipo
    {
        Disponible,
        Asignado,
        EnMantenimiento,
        Dañado
    }
    public enum TipoReporte
    {
        Sala,
        Equipo,
        Otro
    }
    public enum EstadoReporte
    {
        Pendiente,
        EnProceso,
        Cerrado,
        Rechazado // Por si el coordinador considera que no es un daño real
    }
    public enum EstadoReserva
    {
        Pendiente, // Recién creada por el usuario
        Aprobada,  // Aceptada por el coordinador
        Rechazada, // Negada por el coordinador
        EnUso,     // El usuario tiene el equipo
        Finalizada // El usuario liberó el equipo
    }

    public enum TipoReserva
    {
        Equipo,
        Sala
    }
    public enum EstadoSala
    {
        Disponible,
        Ocupada,
        EnMantenimiento,
        Deshabilitada
    }

}
