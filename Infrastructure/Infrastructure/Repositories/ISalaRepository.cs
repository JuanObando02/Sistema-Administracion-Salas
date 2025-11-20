using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public interface ISalaRepository
    {
        // Definición de métodos para la gestión de Salas
        
        Task Save(Sala sala);
        Task<Sala> GetSala(Guid id);
        Task Update(Sala sala);
        Task<Sala> GetSalaConEquipos(Guid id); // Para la validación
        Task Delete(Sala sala);
        Task<IList<Sala>> GetSalas(int? numero = null);
        Task<Sala> GetSalaPorNumero(int numero);
        Task<IList<Sala>> GetSalasIndividuales();// salas para prestamo de equipos
        Task<IList<Sala>> GetSalasClaseCompleta();// salas para clases completas

    }
}
