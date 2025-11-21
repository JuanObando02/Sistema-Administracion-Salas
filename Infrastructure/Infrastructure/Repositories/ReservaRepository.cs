using Domain;
using Domain.Enums;
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
        public async Task<IList<Reserva>> GetReservasActivasEnHorario(DateTime fechaHora)
        {
            // Busca reservas donde la hora actual esté DENTRO del rango de inicio y fin
            return await context.Reservas
                .Where(r => r.FechaInicio <= fechaHora &&
                            r.FechaFin > fechaHora &&
                            (r.Estado == Domain.Enums.EstadoReserva.Aprobada ||
                             r.Estado == Domain.Enums.EstadoReserva.EnUso))
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
        public async Task<bool> TieneReservasActivasOFuturas(Guid equipoId)
        {
            // Retorna TRUE si encuentra al menos una reserva que cumpla las condiciones
            return await context.Reservas.AnyAsync(r =>
                r.EquipoId == equipoId &&
                r.FechaFin > DateTime.Now && // Que no haya terminado todavía
                (r.Estado == Domain.Enums.EstadoReserva.Aprobada ||
                 r.Estado == Domain.Enums.EstadoReserva.EnUso) // Que sea válida
            );
        }
        public async Task Update(Reserva reserva)
        {
            try
            {
                await Beguin();
                // No usamos AddAsync, usamos Update para modificar lo que ya existe
                context.Reservas.Update(reserva);
                await Save();
                await Comit();
            }
            catch (Exception ex)
            {
                await RollBack();
                throw; // 'throw;' es mejor que 'throw ex;' porque mantiene el rastro del error original
            }
        }
        public async Task<IList<Reserva>> GetTodasLasReservas()
        {
            return await context.Reservas
                .Include(r => r.Sala)   // Cargar datos de la sala
                .Include(r => r.Equipo) // Cargar datos del equipo
                .OrderByDescending(r => r.FechaInicio) // Ordenar por fecha (lo más nuevo primero)
                .ToListAsync();
        }
        public async Task<(IList<Reserva> Items, int TotalCount)> GetReservasConFiltros(
            string? busqueda,
            TipoReserva? tipo,
            DateTime? fecha,
            string orden,
            int pagina,
            int pageSize)
        {
            // 1. Empezamos la consulta (AsQueryable no ejecuta nada todavía)
            var query = context.Reservas
                .Include(r => r.Sala)
                .Include(r => r.Equipo)
                .AsQueryable();

            // 2. Aplicar Filtros
            if (tipo.HasValue)
            {
                query = query.Where(r => r.Tipo == tipo.Value);
            }

            if (fecha.HasValue)
            {
                // Filtramos por el día exacto
                query = query.Where(r => r.FechaInicio.Date == fecha.Value.Date);
            }

            if (!string.IsNullOrEmpty(busqueda))
            {
                // Busca en el número de la sala O en el serial del equipo
                // (Transformamos a string para comparar)
                query = query.Where(r =>
                    (r.Sala != null && r.Sala.Numero.ToString().Contains(busqueda)) ||
                    (r.Equipo != null && r.Equipo.Serial.Contains(busqueda))
                );
            }

            // 3. Aplicar Ordenamiento
            query = orden switch
            {
                "tipo_asc" => query.OrderBy(r => r.Tipo),
                "tipo_desc" => query.OrderByDescending(r => r.Tipo),
                "fecha_asc" => query.OrderBy(r => r.FechaInicio),
                _ => query.OrderByDescending(r => r.FechaInicio) // Por defecto: Fecha Descendente
            };

            // 4. Contar Total (para la paginación)
            int total = await query.CountAsync();

            // 5. Paginación y Ejecución
            var items = await query
                .Skip((pagina - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }
        public async Task<bool> ExisteConflicto(Guid? salaId, Guid? equipoId, DateTime inicio, DateTime fin, Guid? reservaIdExcluir = null)
        {
            return await context.Reservas.AnyAsync(r =>
                // 1. Que sea válida (no rechazada ni finalizada)
                r.Estado != Domain.Enums.EstadoReserva.Rechazada &&
                r.Estado != Domain.Enums.EstadoReserva.Finalizada &&

                // 2. Que NO sea la misma reserva que estamos editando
                (reservaIdExcluir == null || r.Id != reservaIdExcluir) &&

                // 3. Que coincida el Recurso (Sala O Equipo)
                (
                    (salaId.HasValue && r.SalaId == salaId) ||
                    (equipoId.HasValue && r.EquipoId == equipoId)
                ) &&

                // 4. LA FÓRMULA DE CRUCE DE HORARIOS
                (inicio < r.FechaFin && fin > r.FechaInicio)
            );
        }

        public async Task<List<Guid>> GetIdsEquiposOcupados(Guid salaId, DateTime inicio, DateTime fin, Guid? reservaIdExcluir = null)
        {
            return await context.Reservas
                .Where(r =>
                    r.SalaId == salaId && // De esta sala
                    r.Tipo == Domain.Enums.TipoReserva.Equipo && // Que sean reservas de equipos
                    r.EquipoId.HasValue &&

                    // Que estén activas
                    r.Estado != Domain.Enums.EstadoReserva.Rechazada &&
                    r.Estado != Domain.Enums.EstadoReserva.Finalizada &&

                    // Que NO sea la reserva que estoy editando (si aplica)
                    (reservaIdExcluir == null || r.Id != reservaIdExcluir) &&

                    // CRUCE DE HORARIOS: (InicioA < FinB) Y (FinA > InicioB)
                    (inicio < r.FechaFin && fin > r.FechaInicio)
                )
                .Select(r => r.EquipoId.Value) // Solo quiero los IDs
                .ToListAsync();
        }
    }
}
