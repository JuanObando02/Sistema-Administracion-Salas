using AutoMapper;
using Domain;
using Domain.Enums;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc.Rendering;
using Services.Models.ReporteModels;
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
            // Validación de Lógica: Si es Equipo, debe tener EquipoId
            if (model.Tipo == TipoReporte.Equipo && !model.EquipoId.HasValue)
            {
                throw new InvalidOperationException("Debe seleccionar el equipo dañado.");
            }
            // Si es Sala, debe tener SalaId
            if (model.Tipo == TipoReporte.Sala && !model.SalaId.HasValue)
            {
                throw new InvalidOperationException("Debe seleccionar la sala afectada.");
            }

            var reporte = new Reporte
            {
                Tipo = model.Tipo,
                Descripcion = model.Descripcion,
                UsuarioId = usuarioId, // El usuario logueado
                FechaCreacion = DateTime.Now,
                Estado = EstadoReporte.Pendiente, // Siempre nace pendiente

                SalaId = model.SalaId,
                EquipoId = (model.Tipo == TipoReporte.Equipo) ? model.EquipoId : null
            };

            await _reporteRepository.Save(reporte);

        }
        public async Task<IList<ReporteIndexModel>> GetMisReportes(string usuarioId)
        {
            var reportes = await _reporteRepository.GetReportesPorUsuario(usuarioId);
            return _mapper.Map<IList<ReporteIndexModel>>(reportes);
        }
    }
}
