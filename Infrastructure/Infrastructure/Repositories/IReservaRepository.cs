using Domain;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public interface IReservaRepository
    {
        Task<IList<Reserva>> GetReservasActivasEnHorario(DateTime fechaHora);
        Task<IList<Reserva>> GetReservasPorUsuario(string usuarioId);
        Task<int> ContarReservasDelDia(string usuarioId, DateTime fecha);
        Task<IList<Reserva>> GetReservasDeSalaPorFecha(Guid salaId, DateTime fecha);
        Task Save(Reserva reserva);
        Task<Reserva> GetReservaCompleta(Guid id); // Obtiene con Includes
        Task Delete(Reserva reserva);
        Task<IList<Reserva>> GetReservasActivasDelUsuarioEnFecha(string usuarioId, DateTime fecha); //reservas activas del usuario ese dia
        Task Update(Reserva reserva);
        Task<bool> TieneReservasActivasOFuturas(Guid equipoId);
        Task<IList<Reserva>> GetTodasLasReservas();
        Task<(IList<Reserva> Items, int TotalCount)> GetReservasConFiltros(
            string? busqueda,
            TipoReserva? tipo,
            DateTime? fecha,
            string orden,
            int pagina,
            int pageSize);    
        Task<bool> ExisteConflicto(Guid? salaId, Guid? equipoId, DateTime inicio, DateTime fin, Guid? reservaIdExcluir = null);

        Task<IList<Reserva>> GetReservasPendientes();
        Task<List<Guid>> GetIdsEquiposOcupados(Guid salaId, DateTime inicio, DateTime fin, Guid? reservaIdExcluir = null);
    }
}
