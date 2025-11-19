using AutoMapper;
using Domain;
using Domain.Enums;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc.Rendering;
using Services.Models.ReservaModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class ReservaService : IReservaService
    {
        private readonly IReservaRepository _reservaRepository;
        private readonly ISalaRepository _salaRepository; // Para el dropdown
        private readonly IEquipoRepository _equipoRepository; // Para buscar equipo
        private readonly IMapper _mapper;

        public ReservaService(
            IReservaRepository reservaRepository,
            ISalaRepository salaRepository,
            IEquipoRepository equipoRepository,
            IMapper mapper)
        {
            _reservaRepository = reservaRepository;
            _salaRepository = salaRepository;
            _equipoRepository = equipoRepository;
            _mapper = mapper;
        }

        public async Task<IList<ReservaIndexModel>> GetMisReservas(string usuarioId)
        {
            var reservas = await _reservaRepository.GetReservasPorUsuario(usuarioId);
            return _mapper.Map<IList<ReservaIndexModel>>(reservas);
        }
        public async Task<ReservarEquipoModel> GetDatosParaReservarEquipo()
        {
            var salas = await _salaRepository.GetSalasIndividuales();
            var modelo = new ReservarEquipoModel
            {
                SalasDisponibles = salas.Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = $"Sala {s.Numero}"
                })
            };
            return modelo;
        }
        public async Task CrearReservaEquipo(ReservarEquipoModel model, string usuarioId)
        {
            // === Max 2 al día ===
            var numReservasHoy = await _reservaRepository.ContarReservasDelDia(usuarioId, model.FechaInicio);
            if (numReservasHoy >= 2)
            {
                throw new InvalidOperationException("Límite excedido: Solo puede realizar 2 reservas por día.");
            }

            // === Horario 7am - 6pm ===
            var horaInicio = model.FechaInicio.Hour;
            var fechaFin = model.FechaInicio.AddHours(model.DuracionHoras);

            if (horaInicio < 7 || horaInicio >= 18)
            {
                throw new InvalidOperationException("Error: Las reservas solo pueden iniciar entre las 7:00 AM y las 6:00 PM.");
            }
            if (fechaFin.Hour > 18 || (fechaFin.Hour == 18 && fechaFin.Minute > 0))
            {
                throw new InvalidOperationException("Error: La reserva no puede terminar después de las 6:00 PM.");
            }

            // === Encontrar un equipo disponible ===
            var sala = await _salaRepository.GetSalaConEquipos(model.SalaId); //
            var reservasEnSala = await _reservaRepository.GetReservasDeSalaPorFecha(model.SalaId, model.FechaInicio);

            Equipo equipoDisponible = null;

            // Itera sobre cada equipo de la sala
            foreach (var equipo in sala.Equipos)
            {
                // Revisa si este equipo tiene reservas que se crucen
                bool estaOcupado = reservasEnSala
                    .Where(r => r.EquipoId == equipo.Id)
                    .Any(r => (model.FechaInicio < r.FechaFin && fechaFin > r.FechaInicio)); // Lógica de cruce

                if (!estaOcupado)
                {
                    equipoDisponible = equipo; // ¡Encontramos uno!
                    break;
                }
            }

            if (equipoDisponible == null)
            {
                throw new InvalidOperationException("No hay equipos disponibles en esa sala para el horario seleccionado.");
            }

            // === 4. Si todo pasa, CREAR LA RESERVA ===
            var reserva = new Reserva
            {
                Tipo = TipoReserva.Equipo,
                FechaInicio = model.FechaInicio,
                FechaFin = fechaFin,
                Estado = EstadoReserva.Aprobada, // Las de equipo se aprueban auto
                UsuarioId = usuarioId,
                SalaId = model.SalaId,
                EquipoId = equipoDisponible.Id // ¡Asignamos el equipo!
            };

            await _reservaRepository.Save(reserva);

            await ActualizarEstadoSalaIndividual(model.SalaId);

        }

        public async Task CancelarReserva(Guid reservaId, string usuarioId)
        {
            // 1. Obtiene la reserva con sus relaciones (Equipo y Sala)
            var reserva = await _reservaRepository.GetReservaCompleta(reservaId);

            if (reserva == null)
            {
                throw new Exception("Reserva no encontrada.");
            }

            // 2. REGLAS DE SEGURIDAD Y ESTADO

            // El usuario logueado debe ser el dueño (Añadir lógica de Administrador/Coordinador más tarde)
            if (reserva.UsuarioId != usuarioId)
            {
                throw new UnauthorizedAccessException("No tiene permiso para cancelar esta reserva.");
            }

            // No se puede cancelar si está en uso o finalizada (EstadoEnUso o Finalizada)
            if (reserva.Estado == EstadoReserva.EnUso || reserva.Estado == EstadoReserva.Finalizada)
            {
                throw new InvalidOperationException("No se puede cancelar una reserva que ya está en uso o finalizada.");
            }

            // 3. REVERSIÓN DE ESTADOS

            // --- Caso 1: Reserva de Equipo Individual ---
            if (reserva.Tipo == TipoReserva.Equipo)
            {
                // Marcamos el equipo como disponible (si existe)
                if (reserva.Equipo != null)
                {
                    reserva.Equipo.Estado = EstadoEquipo.Disponible;
                    await _equipoRepository.Update(reserva.Equipo);
                }

                // Llama al método auxiliar para actualizar el estado de la sala.
                // Esto es necesario para ver si la sala pasa de 'Ocupada' a 'Disponible'.
                if (reserva.SalaId.HasValue)
                {
                    await ActualizarEstadoSalaIndividual(reserva.SalaId.Value);
                }
            }

            // --- Caso 2: Reserva de Sala Completa (Profesor) ---
            else if (reserva.Tipo == TipoReserva.Sala && reserva.Sala != null)
            {
                // Marcamos la sala como disponible inmediatamente
                reserva.Sala.Estado = EstadoSala.Disponible;
                await _salaRepository.Update(reserva.Sala);
            }

            // 4. ELIMINAR LA RESERVA
            await _reservaRepository.Delete(reserva);
        }
        public async Task ActualizarEstadoSalaIndividual(Guid salaId)
        {
            // 1. Obtiene la sala CON sus equipos
            var sala = await _salaRepository.GetSalaConEquipos(salaId); //

            // Solo procesamos salas Individuales
            if (sala == null || sala.Tipo != TipoSala.Individual)
            {
                return;
            }

            // 2. Cuenta cuántos equipos NO están disponibles
            // Asignado, EnMantenimiento, o Dañado son considerados "ocupados"
            int equiposOcupados = sala.Equipos.Count(e => e.Estado != EstadoEquipo.Disponible);

            // 3. Aplica la lógica de estado:
            if (equiposOcupados >= sala.Capacidad) //
            {
                sala.Estado = EstadoSala.Ocupada; // Marca como ocupada
            }
            else
            {
                sala.Estado = EstadoSala.Disponible; // Marca como disponible
            }

            // 4. Guarda el cambio de estado en la BD
            await _salaRepository.Update(sala); //
        }
    }
}
