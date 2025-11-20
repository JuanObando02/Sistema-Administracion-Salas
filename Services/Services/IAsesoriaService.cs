using Services.Models.AsesoriaModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IAsesoriaService
    {
        Task<RegistrarAsesoriaModel> GetDatosParaRegistrar();
        Task CrearAsesoria(RegistrarAsesoriaModel model, string usuarioId);
        Task<IList<AsesoriaIndexModel>> GetMisAsesorias(string usuarioId);
    }
}
