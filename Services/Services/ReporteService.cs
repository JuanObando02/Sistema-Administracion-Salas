using AutoMapper;
using Domain;
using Domain.Enums;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc.Rendering;
using Services.Models.ReporteModels;
using Services.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class ReporteService : IReporteService
    {
        private readonly IReporteRepository _reporteRepository;
        private readonly ISalaRepository _salaRepository;
        private readonly IEquipoRepository _equipoRepository;
        private readonly IMapper _mapper;

        public ReporteService(
            IReporteRepository reporteRepository,
            ISalaRepository salaRepository,
            IEquipoRepository equipoRepository,
            IMapper mapper)
        {
            _reporteRepository = reporteRepository;
            _salaRepository = salaRepository;
            _equipoRepository = equipoRepository;
            _mapper = mapper;
        }

        public async Task<CrearReporteModel> GetDatosParaReportar(Guid? salaId = null, Guid? equipoId = null)
        {
            var modelo = new CrearReporteModel();

            // 1. Cargar lista de Salas (Siempre necesaria)
            var salas = await _salaRepository.GetSalas();
            modelo.SalasDisponibles = salas.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = $"Sala {s.Numero}"
            });

            // 2. Lógica de Pre-llenado si viene un EQUIPO
            if (equipoId.HasValue)
            {
                var equipo = await _equipoRepository.GetEquipo(equipoId.Value);
                if (equipo != null)
                {
                    modelo.Tipo = TipoReporte.Equipo; //
                    modelo.SalaId = equipo.SalaId;    // Pre-selecciona la sala del equipo
                    modelo.EquipoId = equipo.Id;      // Pre-selecciona el equipo

                    // IMPORTANTE: Debemos cargar la lista de equipos de esa sala
                    // para que el dropdown de equipos no aparezca vacío
                    var equiposDeLaSala = await _equipoRepository.GetEquiposPorSala(equipo.SalaId);
                    modelo.EquiposDisponibles = equiposDeLaSala.Select(e => new SelectListItem
                    {
                        Value = e.Id.ToString(),
                        Text = $"{e.Serial} - {e.Estado}"
                    });
                }
            }
            // 3. Lógica de Pre-llenado si viene solo SALA
            else if (salaId.HasValue)
            {
                modelo.Tipo = TipoReporte.Sala; //
                modelo.SalaId = salaId.Value;   // Pre-selecciona la sala
            }

            return modelo;
        }

        public async Task<CrearReporteModel> GetDatosParaReportar()
        {
            // Cargamos solo las salas para empezar
            var salas = await _salaRepository.GetSalas();

            return new CrearReporteModel
            {
                SalasDisponibles = salas.Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = $"Sala {s.Numero}"
                })
            };
        }

        public async Task<IEnumerable<SelectListItem>> GetEquiposPorSalaParaDropdown(Guid salaId)
        {
            var equipos = await _equipoRepository.GetEquiposPorSala(salaId);
            return equipos.Select(e => new SelectListItem
            {
                Value = e.Id.ToString(),
                Text = $"{e.Serial} - {e.Estado}"
            });
        }

        public async Task CrearReporte(CrearReporteModel model, string usuarioId)
        {
            // 1. Validación para EQUIPO
            if (model.Tipo == TipoReporte.Equipo)
            {
                if (!model.SalaId.HasValue) throw new InvalidOperationException("Debe seleccionar la sala.");
                if (!model.EquipoId.HasValue) throw new InvalidOperationException("Debe seleccionar el equipo dañado.");
            }

            // 2. Validación para SALA
            else if (model.Tipo == TipoReporte.Sala)
            {
                if (!model.SalaId.HasValue) throw new InvalidOperationException("Debe seleccionar la sala afectada.");
            }// 3. Validación para OTRO
            else if (model.Tipo == TipoReporte.Otro)
            {

            }

            var reporte = new Reporte
            {
                Tipo = model.Tipo,
                Descripcion = model.Descripcion,
                UsuarioId = usuarioId,
                FechaCreacion = DateTime.Now,
                Estado = EstadoReporte.Pendiente,

                SalaId = (model.Tipo != TipoReporte.Otro) ? model.SalaId : null,
                EquipoId = (model.Tipo == TipoReporte.Equipo) ? model.EquipoId : null
            };

            await _reporteRepository.Save(reporte);

        }
        public async Task<IList<ReporteIndexModel>> GetMisReportes(string usuarioId)
        {
            var reportes = await _reporteRepository.GetReportesPorUsuario(usuarioId);
            return _mapper.Map<IList<ReporteIndexModel>>(reportes);
        }
        public async Task<PaginatedList<ReporteAdminIndexModel>> GetReportesGestionar(FiltroReporteModel filtro)
        {
            // 1. Llamar al repositorio con filtros
            var resultado = await _reporteRepository.GetReportesConFiltros(
                filtro.Busqueda,
                filtro.Estado,
                filtro.Fecha,
                filtro.Pagina,
                10 // Registros por página
            );

            // 2. Mapear a la lista Admin
            var modelos = _mapper.Map<List<ReporteAdminIndexModel>>(resultado.Items);

            // 3. Devolver paginado
            return new PaginatedList<ReporteAdminIndexModel>(modelos, resultado.TotalCount, filtro.Pagina, 10);
        }

        public async Task AtenderReporte(Guid id)
        {
            // 1. Buscar reporte
            // (Necesitas un método GetReporte en tu repositorio que use FindAsync o FirstOrDefault)
            var reporte = await _reporteRepository.GetReportePorId(id);

            if (reporte == null) throw new Exception("Reporte no encontrado.");

            // 2. Validar estado
            if (reporte.Estado != EstadoReporte.Pendiente)
            {
                throw new InvalidOperationException("Solo se pueden atender reportes que estén pendientes.");
            }

            // 3. Cambiar estado
            reporte.Estado = EstadoReporte.EnProceso;

            // 4. Guardar
            await _reporteRepository.Update(reporte);
        }

        public async Task CerrarReporte(Guid id, string observaciones)
        {
            var reporte = await _reporteRepository.GetReportePorId(id);

            if (reporte == null) throw new Exception("Reporte no encontrado.");

            // Solo se cierran los que están en proceso (o pendientes si quieres saltar pasos)
            if (reporte.Estado == EstadoReporte.Cerrado)
            {
                throw new InvalidOperationException("El reporte ya está cerrado.");
            }

            // 1. Cambiar estado
            reporte.Estado = EstadoReporte.Cerrado;

            // 2. Guardar observaciones (Concatenamos a la descripción original para no perder historial)
            if (!string.IsNullOrWhiteSpace(observaciones))
            {
                reporte.Descripcion += $"\n\n[Solución]: {observaciones}";
            }

            // 3. Guardar
            await _reporteRepository.Update(reporte);

            // (Opcional) Si era un equipo dañado, aquí podrías preguntar si quieres volverlo a poner "Disponible"
            // if (reporte.EquipoId.HasValue) { ... lógica para reactivar equipo ... }
        }
        public async Task<IList<ReporteAdminIndexModel>> GetReportesPendientes()
        {
            var reportes = await _reporteRepository.GetReportesPendientes();
            return _mapper.Map<IList<ReporteAdminIndexModel>>(reportes);
        }
        public async Task RechazarReporte(Guid id)
        {
            var reporte = await _reporteRepository.GetReportePorId(id);
            if (reporte == null) throw new Exception("Reporte no encontrado.");

            if (reporte.Estado != EstadoReporte.Pendiente)
            {
                throw new InvalidOperationException("Solo se pueden rechazar reportes pendientes.");
            }

            reporte.Estado = EstadoReporte.Rechazado;
            await _reporteRepository.Update(reporte);
        }
    }
}
