using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public interface IEquipoRepository
    {
        Task Save(Equipo equipo);
        Task<Equipo> GetEquipo(Guid id);
        Task<IList<Equipo>> GetEquipos(); // Para el Index
        Task Update(Equipo equipo);
        Task Delete(Equipo equipo);
        Task<Equipo> GetEquipoPorSerial(string serial);
        Task<IList<Equipo>> GetEquiposPorSala(Guid salaId);
    }
}
