using Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class SalaRepository : BaseRepository, ISalaRepository
    {
        public SalaRepository(AppDbContext context) :base(context) 
        {
            Console.WriteLine("Se crea un repositorio de salas");
        }
        public async Task Save(Sala sala)
        {
            try
            {
                await Beguin(); // Usamos tu transacción
                await context.Salas.AddAsync(sala);
                await Save(); // guardo los cambios
                await Comit();
            }
            catch (Exception ex)
            {
                await RollBack();
                throw ex;
            }
        }
        public async Task<Sala> GetSala(Guid id)
        {
            return await context.Salas.FindAsync(id);
        }
        public async Task Update(Sala sala)
        {
            try
            {
                await Beguin(); // Usamos tu transacción
                context.Salas.Update(sala);
                await Save(); // guardo los cambios
                await Comit();
            }
            catch (Exception ex)
            {
                await RollBack();
                throw ex;
            }
        }
        public async Task<Sala> GetSalaConEquipos(Guid id)
        {
            return await context.Salas
                .Include(sala => sala.Equipos) // ¡Importante para la regla de negocio!
                .FirstOrDefaultAsync(sala => sala.Id == id);
        }
        public async Task Delete(Sala sala)
        {
            try
            {
                await Beguin();
                context.Salas.Remove(sala); // Marca la sala para ser eliminada
                await Save(); // guardo los cambios
                await Comit();
                
            }
            catch (Exception ex)
            {
                await RollBack();
                throw ex;
            }
        }
        public async Task<IList<Sala>> GetSalas()
        {
            return await context.Salas
                .OrderBy(sala => sala.Numero)
                .ToListAsync();
        }
    }
}
