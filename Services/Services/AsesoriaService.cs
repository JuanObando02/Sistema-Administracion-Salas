using AutoMapper;
using Domain;
using Domain.Enums;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc.Rendering;
using Services.Models.AsesoriaModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class AsesoriaService : IAsesoriaService
    {
        private readonly IAsesoriaRepository _asesoriaRepository;
        private readonly ISalaRepository _salaRepository; // Para llenar el dropdown
        private readonly IMapper _mapper;

        public AsesoriaService(IAsesoriaRepository asesoriaRepository, ISalaRepository salaRepository, IMapper mapper)
        {
            _asesoriaRepository = asesoriaRepository;
            _salaRepository = salaRepository;
            _mapper = mapper;
        }

        public async Task<RegistrarAsesoriaModel> GetDatosParaRegistrar(Guid? salaId = null)
        {
            var salas = await _salaRepository.GetSalas();
            var model = new RegistrarAsesoriaModel
            {
                SalasDisponibles = salas.Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = $"Sala {s.Numero}"
                })
            };
            if (salaId.HasValue)
            {
                model.SalaId = salaId.Value;
            }
            return model;
        }

        public async Task CrearAsesoria(RegistrarAsesoriaModel model, string usuarioId)
        {
            var asesoria = new Asesoria
            {
                Descripcion = model.Descripcion,
                SalaId = model.SalaId,
                UsuarioId = usuarioId,
                FechaSolicitud = DateTime.Now,
                Estado = EstadoAsesoria.Pendiente, // Siempre nace pendiente
                CoordinadorId = null
            };

            await _asesoriaRepository.Save(asesoria);
        }

        public async Task<IList<AsesoriaIndexModel>> GetMisAsesorias(string usuarioId)
        {
            var lista = await _asesoriaRepository.GetPorUsuario(usuarioId);
            return _mapper.Map<IList<AsesoriaIndexModel>>(lista);
        }
    }
}
