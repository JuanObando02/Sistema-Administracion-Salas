using Domain;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Infrastructure.Repositories
{
    public class ReporteRepository : BaseRepository, IReporteRepository
    {
        public ReporteRepository(AppDbContext context) : base(context)
        {
        }

        public async Task Save(Reporte reporte)
        {
            try
            {
                await Beguin();
                await context.Reportes.AddAsync(reporte);
                await Save();
                await Comit();
            }
            catch (Exception ex)
            {
                await RollBack();
                throw ex;
            }
        }
        public async Task<IList<Reporte>> GetReportesPorUsuario(string usuarioId)
        {
            return await context.Reportes
                .Where(r => r.UsuarioId == usuarioId)
                .Include(r => r.SalaReportada)   // Cargar datos de la sala
                .Include(r => r.EquipoReportado) // Cargar datos del equipo
                .OrderByDescending(r => r.FechaCreacion) // Los más recientes primero
                .ToListAsync();
        }
        public async Task<(IList<Reporte> Items, int TotalCount)> GetReportesConFiltros(
    string? busqueda,
    Domain.Enums.EstadoReporte? estado,
    DateTime? fecha,
    int pagina,
    int pageSize)
        {
            var query = context.Reportes
                .Include(r => r.SalaReportada)
                .Include(r => r.EquipoReportado)
                .Include(r => r.UsuarioCreador) // Para ver quién reportó
                .AsQueryable();

            // Filtros
            if (estado.HasValue) query = query.Where(r => r.Estado == estado.Value);
            if (fecha.HasValue) query = query.Where(r => r.FechaCreacion.Date == fecha.Value.Date);

            if (!string.IsNullOrEmpty(busqueda))
            {
                query = query.Where(r =>
                    (r.SalaReportada != null && r.SalaReportada.Numero.ToString().Contains(busqueda)) ||
                    (r.EquipoReportado != null && r.EquipoReportado.Serial.Contains(busqueda)) ||
                    r.Descripcion.Contains(busqueda));
            }

            int total = await query.CountAsync();

            var items = await query
                .OrderByDescending(r => r.FechaCreacion)
                .Skip((pagina - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task Update(Reporte reporte)
        {
            try { await Beguin(); context.Reportes.Update(reporte); await Save(); await Comit(); }
            catch (Exception ex) { await RollBack(); throw; }
        }
        public async Task<Reporte> GetReportePorId(Guid id)
        {
            return await context.Reportes.FindAsync(id);
        }

    }
}