using AutoMapper;
using Domain;
using Infrastructure.Repositories;
using Services.Models.EquipoModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class EquipoService : IEquipoService
    {
        private readonly IEquipoRepository _equipoRepository;
        private readonly ISalaRepository _salaRepository; // Para el dropdown
        private readonly IMapper _mapper;

        public async Task<IList<EquipoIndexModel>> GetEquiposPorSala(Guid salaId)
        {
            var lista = await _equipoRepository.GetEquiposPorSala(salaId);
            return _mapper.Map<IList<EquipoIndexModel>>(lista);
        }
        
        public EquipoService(IEquipoRepository equipoRepository, ISalaRepository salaRepository, IMapper mapper)
        {
            _equipoRepository = equipoRepository;
            _salaRepository = salaRepository;
            _mapper = mapper;
        }
        public async Task<Guid> RegistrarEquipo(RegistrarEquipoModel model)
        {
            var equipoExistente = await _equipoRepository.GetEquipoPorSerial(model.Serial);
            if (equipoExistente != null)
            {
                // ¡Error! El serial ya existe
                throw new InvalidOperationException($"El serial '{model.Serial}' ya está registrado.");
            }

            // 2. Cargar la sala (tu lógica de capacidad que ya tenías)
            var sala = await _salaRepository.GetSalaConEquipos(model.SalaId);
            if (sala == null)
                throw new Exception("La sala seleccionada no existe.");

            if (sala.Equipos.Count >= sala.Capacidad) //
                throw new InvalidOperationException($"La Sala {sala.Numero} ya está llena.");

            // 3. Guardar
            var equipo = _mapper.Map<Equipo>(model);
            await _equipoRepository.Save(equipo);

            return sala.Id;
        }

        public async Task<RegistrarEquipoModel> GetDatosParaRegistrar(Guid? salaId)
        {
            var modelo = new RegistrarEquipoModel();

            if (salaId.HasValue)
            {
                // Si venimos de una sala, pre-seleccionamos el ID
                modelo.SalaId = salaId.Value;
            }
            else
            {
                // Si venimos del "Registrar" genérico, llenamos el dropdown
                var salas = await _salaRepository.GetSalas();
                modelo.SalasDisponibles = salas.Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = $"Sala {s.Numero} (Cap: {s.Capacidad})"
                });
            }
            return modelo;
        }

        public async Task<IList<EquipoIndexModel>> GetEquipos()
        {
            var lista = await _equipoRepository.GetEquipos();
            return _mapper.Map<IList<EquipoIndexModel>>(lista);
        }

        public async Task<EditarEquipoModel> GetEquipoParaEditar(Guid id)
        {
            // 1. Obtener el equipo
            var equipo = await _equipoRepository.GetEquipo(id);
            if (equipo == null) return null;

            // 2. Mapear a modelo de edición
            var modelo = _mapper.Map<EditarEquipoModel>(equipo);

            // 3. Llenar el dropdown de salas
            var salas = await _salaRepository.GetSalas();
            modelo.SalasDisponibles = salas.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = $"Sala {s.Numero}"
            });

            return modelo;
        }

        public async Task<Guid> UpdateEquipo(EditarEquipoModel model)
        {
            var equipoConEseSerial = await _equipoRepository.GetEquipoPorSerial(model.Serial);
            if (equipoConEseSerial != null && equipoConEseSerial.Id != model.Id)
            {
                // ¡Error! El serial existe, y NO es el equipo que estamos editando
                throw new InvalidOperationException($"El serial '{model.Serial}' ya está en uso por otro equipo.");
            }
            var equipoExistente = await _equipoRepository.GetEquipo(model.Id);
            if (equipoExistente == null)
            {
                throw new Exception("El equipo que intenta actualizar no existe.");
            }

            // Si el usuario cambió la SalaId, debemos validar la capacidad de la *nueva* sala.
            if (equipoExistente.SalaId != model.SalaId)
            {
                var nuevaSala = await _salaRepository.GetSalaConEquipos(model.SalaId);
                if (nuevaSala.Equipos.Count >= nuevaSala.Capacidad)
                {
                    throw new InvalidOperationException($"La Sala {nuevaSala.Numero} ya está llena.");
                }
            }

            // 3. Mapear los cambios del modelo a la entidad
            _mapper.Map(model, equipoExistente);
            await _equipoRepository.Update(equipoExistente);

            return equipoExistente.SalaId;
        }

        public async Task DeleteEquipo(Guid id)
        {
            // 1. Obtener el equipo
            var equipo = await _equipoRepository.GetEquipo(id);
            if (equipo == null)
            {
                throw new Exception("El equipo que intenta eliminar no existe.");
            }

            // 2. REGLA DE NEGOCIO: No eliminar si está asignado o reservado
            if (equipo.Estado == Domain.Enums.EstadoEquipo.Asignado) //
            {
                throw new InvalidOperationException("No se puede eliminar un equipo que está actualmente asignado.");
            }
            // (Aquí también deberías revisar si tiene reservas activas)
            // if (equipo.Reservas.Any(r => r.Estado == EstadoReserva.Aprobada)) ...

            // 3. Eliminar
            await _equipoRepository.Delete(equipo);
        }
        public async Task<EditarEquipoModel> RepopularDropdownsParaEditar(EditarEquipoModel model)
        {
            // Llama al repositorio de salas (¡esto es correcto!)
            var salas = await _salaRepository.GetSalas(); //

            // Llena la lista en el modelo que recibimos
            model.SalasDisponibles = salas.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = $"Sala {s.Numero}"
            });

            // Devuelve el modelo ahora completo
            return model;
        }
    }
}
