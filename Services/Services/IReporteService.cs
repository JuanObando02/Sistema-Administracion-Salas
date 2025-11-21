using Microsoft.AspNetCore.Mvc.Rendering;
using Services.Models.ReporteModels;
using Services.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IReporteService
    {
        Task<CrearReporteModel> GetDatosParaReportar();
        Task CrearReporte(CrearReporteModel model, string usuarioId);
        Task<IEnumerable<SelectListItem>> GetEquiposPorSalaParaDropdown(Guid salaId);
        Task<CrearReporteModel> GetDatosParaReportar(Guid? salaId = null, Guid? equipoId = null);
        Task<IList<ReporteIndexModel>> GetMisReportes(string usuarioId);
        Task<PaginatedList<ReporteAdminIndexModel>> GetReportesGestionar(FiltroReporteModel filtro);
        Task AtenderReporte(Guid id); // Pasa a EnProceso
        Task CerrarReporte(Guid id, string observaciones); // Pasa a Cerrado
    }
}
