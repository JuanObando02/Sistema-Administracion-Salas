using Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class AsesoriaRepository : BaseRepository, IAsesoriaRepository
    {
        public AsesoriaRepository(AppDbContext context) : base(context) { }

        public async Task Save(Asesoria asesoria)
        {
            try
            {
                await Beguin();
                await context.Asesorias.AddAsync(asesoria);
                await Save();
                await Comit();
            }
            catch (Exception ex)
            {
                await RollBack();
                throw ex;
            }
        }
        public async Task<IList<Asesoria>> GetPorUsuario(string usuarioId)
        {
            return await context.Asesorias
                .Where(a => a.UsuarioId == usuarioId)
                .Include(a => a.Sala) // Traer el nombre de la sala
                .OrderByDescending(a => a.FechaSolicitud)
                .ToListAsync();
        }
        public async Task<IList<Asesoria>> GetAsesoriasActivas()
        {
            return await context.Asesorias
                .Where(a => a.Estado == Domain.Enums.EstadoAsesoria.Pendiente ||
                            a.Estado == Domain.Enums.EstadoAsesoria.EnProceso)
                .Include(a => a.UsuarioSolicitante) // Para ver quién pide ayuda
                .Include(a => a.Sala)               // Para saber a dónde ir
                .OrderBy(a => a.FechaSolicitud)     // Las más viejas primero (FIFO)
                .ToListAsync();
        }
        public async Task Update(Asesoria asesoria)
        {
            try
            {
                await Beguin();
                context.Asesorias.Update(asesoria);
                await Save();
                await Comit();
            }
            catch (Exception ex)
            {
                await RollBack();
                throw;
            }
        }
        public async Task<Asesoria> GetAsesoria(Guid id)
        {

            return await context.Asesorias.FindAsync(id);
        }
        public async Task<(IList<Asesoria> Items, int TotalCount)> GetHistorialConFiltros(
            string? busqueda,
            Domain.Enums.EstadoAsesoria? estado,
            DateTime? fecha,
            int pagina,
            int pageSize)
        {
            var query = context.Asesorias
                .Include(a => a.UsuarioSolicitante)
                .Include(a => a.Sala)
                .AsQueryable();

            // 1. Filtros
            if (estado.HasValue)
                query = query.Where(a => a.Estado == estado.Value);

            if (fecha.HasValue)
                query = query.Where(a => a.FechaSolicitud.Date == fecha.Value.Date);

            if (!string.IsNullOrEmpty(busqueda))
            {
                // Busca por nombre de usuario, sala o descripción
                query = query.Where(a =>
                    (a.UsuarioSolicitante != null && (a.UsuarioSolicitante.Name.Contains(busqueda) || a.UsuarioSolicitante.LastName.Contains(busqueda))) ||
                    (a.Sala != null && a.Sala.Numero.ToString().Contains(busqueda)) ||
                    a.Descripcion.Contains(busqueda));
            }

            // 2. Conteo Total
            int total = await query.CountAsync();

            // 3. Paginación y Orden (Más recientes primero)
            var items = await query
                .OrderByDescending(a => a.FechaSolicitud)
                .Skip((pagina - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }
    }
}
