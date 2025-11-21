using Services.Models.ReservaModels;
using Services.Models.Shared;
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
        Task CrearReservaEquipo(ReservarEquipoModel model, string usuarioId, bool esProfesor);//bool esProfesor para quitar la restriccion de horas
        Task CancelarReserva(Guid reservaId, string usuarioId);
        Task ActualizarEstadoSalaIndividual(Guid salaId);
        Task<ReservarSalaModel> GetDatosParaReservarSala();
        Task CrearReservaSala(ReservarSalaModel model, string usuarioId);
        Task<IList<ReservaIndexModel>> GetTodasLasReservas();
        Task FinalizarReserva(Guid reservaId, string usuarioId);
        Task AprobarReserva(Guid reservaId, string coordinadorId);
        Task RechazarReserva(Guid reservaId, string coordinadorId);
        Task<EditarReservaAdminModel> GetReservaParaEditarAdmin(Guid id);
        Task ActualizarReservaAdmin(EditarReservaAdminModel model, string coordinadorId);
        Task EliminarReservaAdmin(Guid id);
        Task<EditarReservaAdminModel> RepopularDropdownsEditarAdmin(EditarReservaAdminModel model);
        Task<PaginatedList<ReservaIndexModel>> GetReservasGestionar(FiltroReservaModel filtro);
        Task<IList<ReservaIndexModel>> GetReservasPendientes();
    }
}
