using AutoMapper;
using Domain;
using Domain.Enums;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Services.Models.ReservaModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Services.Models.Shared;

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
        private async Task LimpiarReservasVencidas(IEnumerable<Reserva> reservas)
        {
            var ahora = DateTime.Now;

            foreach (var reserva in reservas)
            {
                // Solo intentamos limpiar si ya venció Y sigue activa
                if (reserva.FechaFin < ahora &&
                   (reserva.Estado == EstadoReserva.Aprobada || reserva.Estado == EstadoReserva.EnUso))
                {
                    // --- BLINDAJE CONTRA ERRORES ---
                    // Envolvemos TODO el proceso de actualización en un try-catch
                    try
                    {
                        reserva.Estado = EstadoReserva.Finalizada;

                        // 1. Liberar sala completa si aplica
                        if (reserva.Tipo == TipoReserva.Sala && reserva.Sala != null)
                        {
                            if (reserva.Sala.Estado == EstadoSala.Ocupada)
                            {
                                reserva.Sala.Estado = EstadoSala.Disponible;
                                // Intentamos actualizar la sala
                                await _salaRepository.Update(reserva.Sala);
                            }
                        }

                        // 2. Si era EQUIPO, recalculamos la sala
                        // Nota: Guardamos primero la reserva para que el cálculo sea real
                        await _reservaRepository.Update(reserva);

                        if (reserva.Tipo == TipoReserva.Equipo && reserva.SalaId.HasValue)
                        {
                            await ActualizarEstadoSalaIndividual(reserva.SalaId.Value);
                        }
                    }
                    catch (Exception)
                    {
                        // 🛑 SILENCIAR ERROR
                        // Si falla la limpieza automática de ESTA reserva (por bloqueo de BD, datos sucios, etc.),
                        // simplemente la ignoramos y seguimos con la siguiente.
                        // No queremos que esto rompa la vista del usuario.

                        // (Opcional: Aquí podrías agregar un _logger.LogError(ex, ...) si tuvieras logger)
                    }
                }
            }

        }
        public async Task<IList<ReservaIndexModel>> GetMisReservas(string usuarioId)
        {
            var reservas = await _reservaRepository.GetReservasPorUsuario(usuarioId);

            // Limpiamos antes de mostrar
            await LimpiarReservasVencidas(reservas);
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
        public async Task CrearReservaEquipo(ReservarEquipoModel model, string usuarioId, bool esProfesor)
        {
            // validaciones iniciales fechas pasadas y limite de reservas diarias
            if (model.FechaInicio < DateTime.Now) throw new InvalidOperationException("Error: No se pueden realizar reservas para fechas pasadas.");
            if (!esProfesor)
            {
                var numReservasHoy = await _reservaRepository.ContarReservasDelDia(usuarioId, model.FechaInicio);
                if (numReservasHoy >= 2) throw new InvalidOperationException("Límite excedido: Solo puede realizar 2 reservas por día.");
            }
            // domingos no se hacen reservas
            if (model.FechaInicio.DayOfWeek == DayOfWeek.Sunday)
            {
                throw new InvalidOperationException("Error: La universidad está cerrada los domingos. Por favor seleccione otro día.");
            }

            // Validación de minutos (deben ser 0 o 30)
            if (model.FechaInicio.Minute != 0 && model.FechaInicio.Minute != 30) throw new InvalidOperationException("Error: Las reservas deben iniciar en horas exactas o medias horas.");

            var horaInicioInt = model.FechaInicio.Hour;
            var fechaFin = model.FechaInicio.AddHours(model.DuracionHoras);

            // Validación de Horario (7am - 6pm)
            if (horaInicioInt < 7 || horaInicioInt >= 18)
            {
                throw new InvalidOperationException("Error: Las reservas solo pueden iniciar entre las 7:00 AM y las 6:00 PM.");
            }
            if (fechaFin.Hour > 18 || (fechaFin.Hour == 18 && fechaFin.Minute > 0))
            {
                throw new InvalidOperationException("Error: La reserva no puede terminar después de las 6:00 PM.");
            }

            // === VALIDAR CRUCE DE HORARIOS DEL USUARIO ===
            var misReservasHoy = await _reservaRepository.GetReservasActivasDelUsuarioEnFecha(usuarioId, model.FechaInicio);

            foreach (var reservaExistente in misReservasHoy)
            {
                bool seCruzan = (model.FechaInicio < reservaExistente.FechaFin) && (fechaFin > reservaExistente.FechaInicio);

                if (seCruzan)
                {
                    throw new InvalidOperationException($"Ya tienes una reserva activa entre las {reservaExistente.FechaInicio:hh:mm tt} y las {reservaExistente.FechaFin:hh:mm tt}.");
                }
            }

            // === Encontrar un equipo disponible ===
            var sala = await _salaRepository.GetSalaConEquipos(model.SalaId);
            var reservasEnSala = await _reservaRepository.GetReservasDeSalaPorFecha(model.SalaId, model.FechaInicio);

            Equipo equipoDisponible = null;

            foreach (var equipo in sala.Equipos)
            {
                // Si el equipo está Dañado, Bloqueado o En Mantenimiento, lo saltamos.
                if (equipo.Estado != EstadoEquipo.Disponible)
                {
                    continue;
                }
                // Revisa si este equipo tiene reservas que se crucen
                bool estaOcupado = reservasEnSala
                    .Where(r => r.EquipoId == equipo.Id)
                    .Any(r => (model.FechaInicio < r.FechaFin && fechaFin > r.FechaInicio));

                if (!estaOcupado)
                {
                    equipoDisponible = equipo;
                    break;
                }
            }

            if (equipoDisponible == null)
            {
                throw new InvalidOperationException("No hay equipos disponibles en esa sala para el horario seleccionado (todos están ocupados o en mantenimiento).");
            }

            var reserva = new Reserva
            {
                Tipo = TipoReserva.Equipo,
                FechaInicio = model.FechaInicio,
                FechaFin = fechaFin,
                Estado = EstadoReserva.Aprobada,
                UsuarioId = usuarioId,
                SalaId = model.SalaId,
                EquipoId = equipoDisponible.Id
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
        public async Task<ReservarSalaModel> GetDatosParaReservarSala()
        {
            var salas = await _salaRepository.GetSalasClaseCompleta(); //
            return new ReservarSalaModel
            {
                SalasDisponibles = salas.Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = $"Sala {s.Numero} (Cap: {s.Capacidad})"
                })
            };
        }
        public async Task CrearReservaSala(ReservarSalaModel model, string usuarioId)
        {
            // Validaciones (Fecha pasada y Minutos exactos)
            if (model.FechaInicio < DateTime.Now) throw new InvalidOperationException("Error: No se pueden realizar reservas para fechas pasadas.");
            if (model.FechaInicio.Minute != 0 && model.FechaInicio.Minute != 30) throw new InvalidOperationException("Error: Las reservas deben iniciar en horas exactas o medias horas.");
            // domingos no se hacen reservas
            if (model.FechaInicio.DayOfWeek == DayOfWeek.Sunday)
            {
                throw new InvalidOperationException("Error: La universidad está cerrada los domingos. Por favor seleccione otro día.");
            }

            var fechaFin = model.FechaInicio.AddHours(model.DuracionHoras);

            //Validar Horario (7am - 9:30pm)
            if (model.FechaInicio.Hour < 7)
            {
                throw new InvalidOperationException("Error: Las salas abren a las 7:00 AM.");
            }
            if (fechaFin.Hour > 21 || (fechaFin.Hour == 21 && fechaFin.Minute > 30))
            {
                throw new InvalidOperationException("Error: La reserva de sala no puede terminar después de las 9:30 PM.");
            }

            // Verifica si el profesor ya tiene otra reserva (de sala o equipo) a esa hora.
            var misReservasHoy = await _reservaRepository.GetReservasActivasDelUsuarioEnFecha(usuarioId, model.FechaInicio);

            foreach (var reservaExistente in misReservasHoy)
            {
                bool seCruzan = (model.FechaInicio < reservaExistente.FechaFin) && (fechaFin > reservaExistente.FechaInicio);

                if (seCruzan)
                {
                    throw new InvalidOperationException($"Conflicto de horario: Ya tienes una reserva activa entre las {reservaExistente.FechaInicio:hh:mm tt} y las {reservaExistente.FechaFin:hh:mm tt}. No puedes estar en dos lugares al mismo tiempo.");
                }
            }

            // Validar Disponibilidad de la Sala (Que la sala no esté ocupada por OTRO profesor)
            var reservasDeLaSala = await _reservaRepository.GetReservasDeSalaPorFecha(model.SalaId, model.FechaInicio);

            bool salaOcupada = reservasDeLaSala.Any(r =>
                r.Estado != EstadoReserva.Rechazada &&
                r.Estado != EstadoReserva.Finalizada &&
                (model.FechaInicio < r.FechaFin && fechaFin > r.FechaInicio));

            if (salaOcupada)
            {
                throw new InvalidOperationException("La sala seleccionada ya se encuentra reservada en ese horario por otra persona.");
            }

            // 5. Crear la Reserva
            var reserva = new Reserva
            {
                Tipo = TipoReserva.Sala,
                FechaInicio = model.FechaInicio,
                FechaFin = fechaFin,
                Estado = EstadoReserva.Pendiente, // Requiere aprobación
                UsuarioId = usuarioId,
                SalaId = model.SalaId,
                EquipoId = null
            };

            await _reservaRepository.Save(reserva);
        }
        public async Task FinalizarReserva(Guid reservaId, string usuarioId)
        {
            // 1. Obtener la reserva
            var reserva = await _reservaRepository.GetReservaCompleta(reservaId);
            if (reserva == null) throw new Exception("Reserva no encontrada.");

            // 2. Validar dueño
            if (reserva.UsuarioId != usuarioId)
            {
                throw new UnauthorizedAccessException("No tiene permiso para finalizar esta reserva.");
            }

            // 3. Validar que esté en curso
            var ahora = DateTime.Now;
            bool enCurso = (reserva.Estado == EstadoReserva.Aprobada || reserva.Estado == EstadoReserva.EnUso)
                           && ahora >= reserva.FechaInicio
                           && ahora < reserva.FechaFin;

            if (!enCurso)
            {
                throw new InvalidOperationException("Solo se pueden finalizar reservas que están en curso actualmente.");
            }

            // 4. Cortamos la reserva
            reserva.FechaFin = DateTime.Now;
            reserva.Estado = EstadoReserva.Finalizada;

            // 5. Actualizar estados colaterales
            if (reserva.Tipo == TipoReserva.Equipo && reserva.SalaId.HasValue)
            {
                // CASO 1: Equipo
                await _reservaRepository.Update(reserva);

                // Recalculamos la sala
                await ActualizarEstadoSalaIndividual(reserva.SalaId.Value);
            }
            else if (reserva.Tipo == TipoReserva.Sala && reserva.Sala != null)
            {
                // CASO 2: Sala Completa
                reserva.Sala.Estado = EstadoSala.Disponible;
                await _salaRepository.Update(reserva.Sala);
                await _reservaRepository.Update(reserva);
            }
            else
            {
                await _reservaRepository.Update(reserva);
            }
        }
        public async Task<IList<ReservaIndexModel>> GetTodasLasReservas()
        {
            var todas = await _reservaRepository.GetTodasLasReservas();

            await LimpiarReservasVencidas(todas);
            return _mapper.Map<IList<ReservaIndexModel>>(todas);
        }
        public async Task AprobarReserva(Guid reservaId, string coordinadorId)
        {
            var reserva = await _reservaRepository.GetReservaCompleta(reservaId);
            if (reserva == null) throw new Exception("Reserva no encontrada.");

            if (reserva.Estado != EstadoReserva.Pendiente)
            {
                throw new InvalidOperationException("Solo se pueden aprobar reservas pendientes.");
            }

            // --- LÓGICA DE CAMBIO DE ESTADO ---

            // CASO A: Es una Sala Completa (Profesor)
            if (reserva.Tipo == TipoReserva.Sala && reserva.Sala != null)
            {
                // Validamos que la sala siga existiendo y sirviendo (Físicamente)
                // PERO NO cambiamos su estado a 'Ocupada' aquí.
                if (reserva.Sala.Estado == EstadoSala.EnMantenimiento ||
                    reserva.Sala.Estado == EstadoSala.Deshabilitada)
                {
                    throw new InvalidOperationException("No se puede aprobar: La sala está en mantenimiento o deshabilitada.");
                }
            }

            reserva.Estado = EstadoReserva.Aprobada;
            await _reservaRepository.Update(reserva);
        }
        public async Task RechazarReserva(Guid reservaId, string coordinadorId)
        {
            var reserva = await _reservaRepository.GetReservaCompleta(reservaId);
            if (reserva == null) throw new Exception("Reserva no encontrada.");

            if (reserva.Estado != EstadoReserva.Pendiente)
            {
                throw new InvalidOperationException("Solo se pueden rechazar reservas que estén en estado Pendiente.");
            }

            // Actualizar estado y firmar
            reserva.Estado = EstadoReserva.Rechazada;
            reserva.AprobadorId = coordinadorId;

            await _reservaRepository.Update(reserva);
        }
        public async Task<EditarReservaAdminModel> GetReservaParaEditarAdmin(Guid id)
        {
            var reserva = await _reservaRepository.GetReservaCompleta(id);
            if (reserva == null) return null;

            var model = new EditarReservaAdminModel
            {
                Id = reserva.Id,
                Tipo = reserva.Tipo,
                FechaInicio = reserva.FechaInicio,
                FechaFin = reserva.FechaFin,
                SalaId = reserva.SalaId,
                EquipoId = reserva.EquipoId
            };

            // Cargar listas según el tipo
            if (reserva.Tipo == TipoReserva.Sala)
            {
                var salas = await _salaRepository.GetSalasClaseCompleta();
                model.SalasDisponibles = salas.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = $"Sala {s.Numero}" });
            }
            else // Equipo
            {
                // Cargamos TODAS las salas y TODOS los equipos (o podrías filtrar por sala si quisieras)
                // Para simplificar, cargamos equipos si hay una sala seleccionada
                var salas = await _salaRepository.GetSalasIndividuales();
                model.SalasDisponibles = salas.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = $"Sala {s.Numero}" });

                if (reserva.SalaId.HasValue)
                {
                    var equipos = await _equipoRepository.GetEquiposPorSala(reserva.SalaId.Value);
                    model.EquiposDisponibles = equipos.Select(e => new SelectListItem { Value = e.Id.ToString(), Text = e.Serial });
                }
            }

            return model;
        }
        public async Task ActualizarReservaAdmin(EditarReservaAdminModel model, string coordinadorId)
        {
            var reserva = await _reservaRepository.GetReservaCompleta(model.Id); //
            if (reserva == null) throw new Exception("Reserva no encontrada.");

            // --- 1. VALIDACIÓN DE CONFLICTO DE HORARIO ---

            // Determinamos qué IDs vamos a verificar
            Guid? equipoIdVerificar = null;
            Guid? salaIdVerificar = null;

            if (reserva.Tipo == TipoReserva.Equipo)
            {
                // Si es equipo, validamos que ESE equipo no esté ocupado
                equipoIdVerificar = model.EquipoId;
            }
            else
            {
                // Si es sala completa, validamos que ESA sala no esté ocupada
                salaIdVerificar = model.SalaId;
            }

            // Llamamos al repositorio
            bool hayConflicto = await _reservaRepository.ExisteConflicto(
                salaIdVerificar,
                equipoIdVerificar,
                model.FechaInicio,
                model.FechaFin,
                model.Id // ¡IMPORTANTE! Pasamos el ID actual para excluirlo
            );

            if (hayConflicto)
            {
                string recurso = reserva.Tipo == TipoReserva.Equipo ? "El equipo" : "La sala";
                throw new InvalidOperationException($"Conflicto: {recurso} seleccionado ya tiene una reserva en ese horario.");
            }
            // ---------------------------------------------

            // 2. Actualizar datos (Tu código existente)
            reserva.FechaInicio = model.FechaInicio;
            reserva.FechaFin = model.FechaFin;
            reserva.SalaId = model.SalaId;
            reserva.EquipoId = model.EquipoId;

            await _reservaRepository.Update(reserva);
        }
        public async Task EliminarReservaAdmin(Guid id)
        {
            var reserva = await _reservaRepository.GetReservaCompleta(id);
            if (reserva != null)
            {
                // Lógica de reversión de estados (si estaba ocupada, liberar sala/equipo)
                if (reserva.Tipo == TipoReserva.Sala && reserva.Sala != null && reserva.Sala.Estado == EstadoSala.Ocupada)
                {
                    reserva.Sala.Estado = EstadoSala.Disponible;
                    await _salaRepository.Update(reserva.Sala);
                }

                await _reservaRepository.Delete(reserva);
            }
        }
        public async Task<PaginatedList<ReservaIndexModel>> GetReservasGestionar(FiltroReservaModel filtro)
        {
            // 1. Llamar al repositorio con los filtros
            var resultado = await _reservaRepository.GetReservasConFiltros(
                filtro.Busqueda,
                filtro.Tipo,
                filtro.Fecha,
                filtro.OrdenarPor,
                filtro.Pagina,
                filtro.RegistrosPorPagina
            );

            // 2. Limpieza de vencidos (la lógica que ya teníamos)
            // Nota: Solo limpiamos la página actual para no afectar rendimiento
            await LimpiarReservasVencidas(resultado.Items);

            // 3. Mapear a ViewModel
            var listaModelos = _mapper.Map<List<ReservaIndexModel>>(resultado.Items);

            // 4. Devolver lista paginada
            return new PaginatedList<ReservaIndexModel>(
                listaModelos,
                resultado.TotalCount,
                filtro.Pagina,
                filtro.RegistrosPorPagina
            );
        }
        public async Task<EditarReservaAdminModel> RepopularDropdownsEditarAdmin(EditarReservaAdminModel model)
        {
            // 1. Recargar Salas según el tipo de reserva
            if (model.Tipo == TipoReserva.Sala)
            {
                var salas = await _salaRepository.GetSalasClaseCompleta(); //
                model.SalasDisponibles = salas.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = $"Sala {s.Numero}" });
            }
            else // Equipo
            {
                var salas = await _salaRepository.GetSalasIndividuales(); //
                model.SalasDisponibles = salas.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = $"Sala {s.Numero}" });

                // 2. Recargar Equipos si hay una sala seleccionada
                if (model.SalaId.HasValue)
                {
                    var equipos = await _equipoRepository.GetEquiposPorSala(model.SalaId.Value); //
                    model.EquiposDisponibles = equipos.Select(e => new SelectListItem
                    {
                        Value = e.Id.ToString(),
                        Text = $"{e.Serial} - {e.Estado}"
                    });
                }
            }

            return model;
        }

    }

}
