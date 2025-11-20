using Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class ReservaRepository : BaseRepository, IReservaRepository
    {
        public ReservaRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<IList<Reserva>> GetReservasActivasDelUsuarioEnFecha(string usuarioId, DateTime fecha)
        {
            return await context.Reservas
                .Where(r => r.UsuarioId == usuarioId &&
                            r.FechaInicio.Date == fecha.Date &&
                            // Ignoramos las que ya se cancelaron o rechazaron
                            r.Estado != Domain.Enums.EstadoReserva.Rechazada &&
                            r.Estado != Domain.Enums.EstadoReserva.Finalizada)
                .ToListAsync();
        }
        public async Task<IList<Reserva>> GetReservasPorUsuario(string usuarioId)
        {
            return await context.Reservas
                .Where(r => r.UsuarioId == usuarioId)
                .Include(r => r.Sala)     // Carga la Sala (para mostrar el nombre)
                .Include(r => r.Equipo)   // Carga el Equipo (para mostrar el serial)
                .OrderByDescending(r => r.FechaInicio)
                .ToListAsync();
        }
        public async Task<int> ContarReservasDelDia(string usuarioId, DateTime fecha)
        {
            // Busca reservas de ese usuario que comiencen en esa fecha
            return await context.Reservas
                .CountAsync(r => r.UsuarioId == usuarioId &&
                                 r.FechaInicio.Date == fecha.Date);
        }
        public async Task<IList<Reserva>> GetReservasDeSalaPorFecha(Guid salaId, DateTime fecha)
        {
            // Carga todas las reservas de una sala para un día
            return await context.Reservas
                .Where(r => r.SalaId == salaId && r.FechaInicio.Date == fecha.Date)
                .ToListAsync();
        }
        public async Task Save(Reserva reserva)
        {
            try
            {
                await Beguin();
                await context.Reservas.AddAsync(reserva);
                await Save();
                await Comit();
            }
            catch (Exception ex)
            {
                await RollBack();
                throw ex;
            }
        }
        public async Task<Reserva> GetReservaCompleta(Guid id)
        {
            // Incluye Equipo y Sala para la lógica de reversión de estado
            return await context.Reservas
                .Include(r => r.Equipo)
                .Include(r => r.Sala)
                .FirstOrDefaultAsync(r => r.Id == id);
        }
        public async Task Delete(Reserva reserva)
        {
            try
            {
                await Beguin();
                context.Reservas.Remove(reserva);
                await Save(); // Llama a SaveChanges()
                await Comit();
            }
            catch (Exception ex)
            {
                await RollBack();
                throw ex;
            }
        }
    }
}
