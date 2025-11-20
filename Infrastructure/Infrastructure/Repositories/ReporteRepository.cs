using Domain;
using Infrastructure.Repositories;
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
    }
}