using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Services.Models.SalaModels;

namespace Services
{
    public interface ISalaService
    {
        Task RegistrarSala(RegistrarSalaModel model);
        Task<EditarSalaModel> GetSalaParaEditar(Guid id);
        Task UpdateSala(EditarSalaModel model);
        Task DeleteSala(Guid id);
        Task<IList<SalaIndexModel>> GetSalas();
        Task<IList<EstadoSalaViewModel>> GetEstadoActualSalas();
    }
}

