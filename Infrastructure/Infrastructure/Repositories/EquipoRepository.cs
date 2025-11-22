using Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class EquipoRepository : BaseRepository, IEquipoRepository
    {

        public EquipoRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<Equipo> GetEquipoPorSerial(string serial)
        {
            // Busca el primer equipo que coincida con el serial (ignorando mayúsculas/minúsculas)
            return await context.Equipos
                .FirstOrDefaultAsync(e => e.Serial.ToUpper() == serial.ToUpper());
        }
        public async Task Save(Equipo equipo)
        {
            try
            {
                await Beguin();
                await context.Equipos.AddAsync(equipo);
                await Save();
                await Comit();
            }
            catch (Exception)
            {
                await RollBack();
            }
        }
        public async Task<Equipo> GetEquipo(Guid id)
        {
            // FindAsync es el más rápido para buscar por ID
            return await context.Equipos.FindAsync(id);
        }

        public async Task<IList<Equipo>> GetEquipos(string? searchSerial = null)
        {
            var query = context.Equipos.AsQueryable();

            if (!string.IsNullOrEmpty(searchSerial))
            {
                // Busca coincidencias parciales (LIKE %search%)
                query = query.Where(e => e.Serial.Contains(searchSerial));
            }

            return await query
                .Include(e => e.Sala)
                .OrderBy(e => e.Serial)
                .ToListAsync();
        }

        public async Task Update(Equipo equipo)
        {
            try
            {
                await Beguin();
                context.Equipos.Update(equipo); // Marca para actualizar
                await Save(); // Llama a SaveChanges()
                await Comit();
            }
            catch (Exception ex)
            {
                await RollBack();
                throw ex;
            }
        }

        public async Task Delete(Equipo equipo)
        {
            try
            {
                await Beguin();
                context.Equipos.Remove(equipo); // Marca para eliminar
                await Save(); // Llama a SaveChanges()
                await Comit();
            }
            catch (Exception ex)
            {
                await RollBack();
                throw ex;
            }
        }

        public async Task<IList<Equipo>> GetEquiposPorSala(Guid salaId, string? searchSerial = null)
        {
            var query = context.Equipos
            .Where(e => e.SalaId == salaId) // Filtro base de sala
            .AsQueryable();

            if (!string.IsNullOrEmpty(searchSerial))
            {
                query = query.Where(e => e.Serial.Contains(searchSerial));
            }

            return await query
                .Include(e => e.Sala)
                .OrderBy(e => e.Serial)
                .ToListAsync();
        }
    }

}
