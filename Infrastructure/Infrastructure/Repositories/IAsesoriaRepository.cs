using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public interface IAsesoriaRepository
    {
        Task Save(Asesoria asesoria);
        Task<IList<Asesoria>> GetPorUsuario(string usuarioId);
        Task<IList<Asesoria>> GetAsesoriasActivas();
        Task Update(Asesoria asesoria);
        Task<Asesoria> GetAsesoria(Guid id);
        Task<(IList<Asesoria> Items, int TotalCount)> GetHistorialConFiltros(
            string? busqueda,
            Domain.Enums.EstadoAsesoria? estado,
            DateTime? fecha,
            int pagina,
            int pageSize);
    }
}
