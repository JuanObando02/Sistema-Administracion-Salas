using AutoMapper;
using Domain;
using Infrastructure.Repositories;
using Services.Models.SalaModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class SalaService : ISalaService
    {
        private readonly ISalaRepository _salaRepository;
        private readonly IMapper _mapper;

        public SalaService(ISalaRepository salaRepository, IMapper mapper)
        {
            _salaRepository = salaRepository;
            _mapper = mapper;
        }

        public async Task RegistrarSala(RegistrarSalaModel model)
        {
            var salaExistente = await _salaRepository.GetSalaPorNumero(model.Numero);
            if (salaExistente != null)
            {
                throw new InvalidOperationException($"El número de sala '{model.Numero}' ya está registrado.");
            }
            var sala = _mapper.Map<Sala>(model);
            await _salaRepository.Save(sala);
        }

        public async Task<EditarSalaModel> GetSalaParaEditar(Guid id)
        {
            var sala = await _salaRepository.GetSala(id);

            // Mapea la Entidad (Sala) al Modelo (EditarSalaModel)
            return _mapper.Map<EditarSalaModel>(sala);
        }

        public async Task UpdateSala(EditarSalaModel model)
        {
            var salaConEseNumero = await _salaRepository.GetSalaPorNumero(model.Numero);
            if (salaConEseNumero != null && salaConEseNumero.Id != model.Id)
            {
                throw new InvalidOperationException($"El número de sala '{model.Numero}' ya está en uso por otra sala.");
            }

            var salaExistente = await _salaRepository.GetSalaConEquipos(model.Id); //
            if (salaExistente == null)
            {
                throw new Exception("La sala que intenta actualizar no existe.");
            }

            if (model.Capacidad < salaExistente.Equipos.Count)
            {
                throw new InvalidOperationException($"Error: No se puede reducir la capacidad a {model.Capacidad} porque la sala ya tiene {salaExistente.Equipos.Count} equipos asignados."); //
            }

            _mapper.Map(model, salaExistente);
            await _salaRepository.Update(salaExistente); ;
            
        }
        public async Task DeleteSala(Guid id)
        {
            // 1. Obtener la sala CON sus equipos
            var sala = await _salaRepository.GetSalaConEquipos(id);

            if (sala == null)
            {
                throw new Exception("La sala que intenta eliminar no existe.");
            }

            // 2. REGLA DE NEGOCIO:
            // (Revisa la lista de equipos que cargamos)
            if (sala.Equipos.Any())
            {
                throw new InvalidOperationException("Error: No se puede eliminar una sala que todavía tiene equipos asignados.");
            }

            // 3. Si la sala está vacía, procede a eliminarla
            await _salaRepository.Delete(sala);
        }
        public async Task<IList<SalaIndexModel>> GetSalas()
        {
            // 1. Llama al repositorio
            var salasList = await _salaRepository.GetSalas();

            // 2. Mapea la lista de Entidades (Sala) a Modelos (SalaIndexModel)
            return _mapper.Map<IList<SalaIndexModel>>(salasList);
        }
    }
}
