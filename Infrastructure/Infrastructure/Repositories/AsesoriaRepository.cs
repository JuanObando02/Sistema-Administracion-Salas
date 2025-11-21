using Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class AsesoriaRepository : BaseRepository, IAsesoriaRepository
    {
        public AsesoriaRepository(AppDbContext context) : base(context) { }

        public async Task Save(Asesoria asesoria)
        {
            try
            {
                await Beguin();
                await context.Asesorias.AddAsync(asesoria);
                await Save();
                await Comit();
            }
            catch (Exception ex)
            {
                await RollBack();
                throw ex;
            }
        }
        public async Task<IList<Asesoria>> GetPorUsuario(string usuarioId)
        {
            return await context.Asesorias
                .Where(a => a.UsuarioId == usuarioId)
                .Include(a => a.Sala) // Traer el nombre de la sala
                .OrderByDescending(a => a.FechaSolicitud)
                .ToListAsync();
        }
        public async Task<IList<Asesoria>> GetAsesoriasActivas()
        {
            return await context.Asesorias
                .Where(a => a.Estado == Domain.Enums.EstadoAsesoria.Pendiente ||
                            a.Estado == Domain.Enums.EstadoAsesoria.EnProceso)
                .Include(a => a.UsuarioSolicitante) // Para ver quién pide ayuda
                .Include(a => a.Sala)               // Para saber a dónde ir
                .OrderBy(a => a.FechaSolicitud)     // Las más viejas primero (FIFO)
                .ToListAsync();
        }
        public async Task Update(Asesoria asesoria)
        {
            try
            {
                await Beguin();
                context.Asesorias.Update(asesoria);
                await Save();
                await Comit();
            }
            catch (Exception ex)
            {
                await RollBack();
                throw;
            }
        }
        public async Task<Asesoria> GetAsesoria(Guid id)
        {

            return await context.Asesorias.FindAsync(id);
        }
    }
}
