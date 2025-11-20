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
    }
}