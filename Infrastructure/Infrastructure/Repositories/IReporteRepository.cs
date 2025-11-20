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
    }
}