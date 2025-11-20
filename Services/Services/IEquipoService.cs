using Services.Models.EquipoModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IEquipoService
    {
        Task<Guid> RegistrarEquipo(RegistrarEquipoModel model);
        Task<RegistrarEquipoModel> GetDatosParaRegistrar(Guid? salaId);
        Task<IList<EquipoIndexModel>> GetEquipos(string? searchSerial = null);
        Task<EditarEquipoModel> GetEquipoParaEditar(Guid id);
        Task<Guid> UpdateEquipo(EditarEquipoModel model);
        Task DeleteEquipo(Guid id);
        Task<EditarEquipoModel> RepopularDropdownsParaEditar(EditarEquipoModel model);
        Task<IList<EquipoIndexModel>> GetEquiposPorSala(Guid salaId, string? searchSerial = null);
    }
}
