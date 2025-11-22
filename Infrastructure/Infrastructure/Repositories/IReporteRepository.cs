using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain;

namespace Infrastructure.Repositories
{
    public interface IReporteRepository
    {
        Task Save(Reporte reporte);
        Task<IList<Reporte>> GetReportesPorUsuario(string usuarioId);
        Task<(IList<Reporte> Items, int TotalCount)> GetReportesConFiltros(
            string? busqueda,
            Domain.Enums.EstadoReporte? estado,
            DateTime? fecha,
            int pagina,
            int pageSize);
        Task Update(Reporte reporte);
        Task<Reporte> GetReportePorId(Guid id);
        Task<IList<Reporte>> GetReportesPendientes();
    }
}