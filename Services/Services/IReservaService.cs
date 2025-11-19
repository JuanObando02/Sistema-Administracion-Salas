using Services.Models.ReservaModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IReservaService
    {
        Task<IList<ReservaIndexModel>> GetMisReservas(string usuarioId);
        Task<ReservarEquipoModel> GetDatosParaReservarEquipo();
        Task CrearReservaEquipo(ReservarEquipoModel model, string usuarioId);
        Task CancelarReserva(Guid reservaId, string usuarioId);
        Task ActualizarEstadoSalaIndividual(Guid salaId);
    }
}
