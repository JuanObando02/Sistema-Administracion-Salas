using AutoMapper;
using Domain;
using Domain.Enums;
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
        private readonly IReservaRepository _reservaRepository;
        private readonly IMapper _mapper;

        public SalaService(ISalaRepository salaRepository, IMapper mapper, IReservaRepository reservaRepository)
        {
            _salaRepository = salaRepository;
            _reservaRepository = reservaRepository;
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
        public async Task<IList<SalaIndexModel>> GetSalas(int? numero = null)
        {
            // Pasamos el número al repositorio
            var salasList = await _salaRepository.GetSalas(numero);

            return _mapper.Map<IList<SalaIndexModel>>(salasList);
        }
        public async Task<IList<EstadoSalaViewModel>> GetEstadoActualSalas()
        {
            var salas = await _salaRepository.GetSalas();
            var listaEstados = new List<EstadoSalaViewModel>();

            // 2. OBTENER LA "FOTO" DEL MOMENTO
            // Traemos todas las reservas que están ocurriendo YA MISMO (DateTime.Now)
            // (Asegúrate de tener este método en tu IReservaRepository como vimos antes)
            var reservasActivasAhora = await _reservaRepository.GetReservasActivasEnHorario(DateTime.Now);

            // Creamos un HashSet de los IDs de equipos ocupados para que la búsqueda sea ultra rápida
            var idsEquiposOcupados = reservasActivasAhora
                .Where(r => r.EquipoId.HasValue)
                .Select(r => r.EquipoId.Value)
                .ToHashSet();

            // Creamos un HashSet de los IDs de salas ocupadas (por profesores)
            var idsSalasOcupadas = reservasActivasAhora
                .Where(r => r.SalaId.HasValue && r.Tipo == TipoReserva.Sala)
                .Select(r => r.SalaId.Value)
                .ToHashSet();

            foreach (var sala in salas)
            {
                // Cargamos la sala con sus equipos
                var salaCompleta = await _salaRepository.GetSalaConEquipos(sala.Id);

                int equiposLibres = 0;
                var estadoCalculado = sala.Estado; // Empezamos con el estado físico base

                // --- LÓGICA PARA SALA INDIVIDUAL (Estudiantes) ---
                if (sala.Tipo == TipoSala.Individual)
                {
                    if (salaCompleta.Equipos != null)
                    {
                        // 3. EL CÁLCULO REAL
                        // Un equipo cuenta como libre SI:
                        // a) Físicamente está 'Disponible'
                        // b) Y NO está en la lista de ocupados ahora mismo
                        equiposLibres = salaCompleta.Equipos.Count(e =>
                            e.Estado == EstadoEquipo.Disponible &&
                            !idsEquiposOcupados.Contains(e.Id));
                    }

                    // Si la sala físicamente está bien, pero se llenó de gente, la mostramos Ocupada
                    if (sala.Estado == EstadoSala.Disponible)
                    {
                        estadoCalculado = equiposLibres == 0 ? EstadoSala.Ocupada : EstadoSala.Disponible;
                    }
                }
                // --- LÓGICA PARA SALA DE CLASE (Profesores) ---
                else
                {
                    // Si la sala está físicamente bien, revisamos si hay un profesor dando clase ahora
                    if (sala.Estado == EstadoSala.Disponible)
                    {
                        bool hayClaseAhora = idsSalasOcupadas.Contains(sala.Id);
                        estadoCalculado = hayClaseAhora ? EstadoSala.Ocupada : EstadoSala.Disponible;
                    }
                }

                // 4. Construimos el modelo para la vista
                listaEstados.Add(new EstadoSalaViewModel
                {
                    Id = sala.Id,
                    NombreSala = $"Sala {sala.Numero}",
                    Capacidad = sala.Capacidad,
                    EquiposDisponibles = equiposLibres, // Este número ahora es REAL (Físico - Ocupados)
                    Estado = estadoCalculado,
                    Tipo = sala.Tipo
                });
            }

            return listaEstados;
        }
    }

}
