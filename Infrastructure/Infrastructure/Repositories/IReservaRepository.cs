using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public interface IReservaRepository
    {
        Task<IList<Reserva>> GetReservasPorUsuario(string usuarioId);
        Task<int> ContarReservasDelDia(string usuarioId, DateTime fecha);
        Task<IList<Reserva>> GetReservasDeSalaPorFecha(Guid salaId, DateTime fecha);
        Task Save(Reserva reserva);
        Task<Reserva> GetReservaCompleta(Guid id); // Obtiene con Includes
        Task Delete(Reserva reserva);
    }
}
