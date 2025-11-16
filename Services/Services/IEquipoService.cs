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
        Task RegistrarEquipo(RegistrarEquipoModel model);
        Task<RegistrarEquipoModel> GetDatosParaRegistrar();
        Task<IList<EquipoIndexModel>> GetEquipos();
        Task<EditarEquipoModel> GetEquipoParaEditar(Guid id);
        Task UpdateEquipo(EditarEquipoModel model);
        Task DeleteEquipo(Guid id);
        Task<EditarEquipoModel> RepopularDropdownsParaEditar(EditarEquipoModel model);
    }
}
