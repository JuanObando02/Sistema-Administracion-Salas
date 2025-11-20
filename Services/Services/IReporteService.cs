using Microsoft.AspNetCore.Mvc.Rendering;
using Services.Models.ReporteModels;
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

        // Método extra para cargar equipos cuando seleccionen una sala (AJAX)
        Task<IEnumerable<SelectListItem>> GetEquiposPorSalaParaDropdown(Guid salaId);
    }
}
