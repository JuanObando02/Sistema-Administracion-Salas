using AutoMapper;
using Domain;
using Domain.Enums;
using Services.Models.AsesoriaModels;
using Services.Models.CowModels;
using Services.Models.EquipoModels;
using Services.Models.FarmModels;
using Services.Models.MilkModels;
using Services.Models.ReporteModels;
using Services.Models.ReservaModels;
using Services.Models.SalaModels;
using Services.Models.UsuarioModels;

namespace Services.Automapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            FarmMapper();
            MilkMapper();
            CowMapper();
            SalaMapper();
            EquipoMapper();
            UsuarioMapper();
            ReservaMapper();
            ReporteMapper();
            AsesoriaMapper();
        }
        private void AsesoriaMapper()
        {
            CreateMap<Asesoria, AsesoriaIndexModel>()
                .ForMember(dest => dest.Fecha, opt => opt.MapFrom(src => src.FechaSolicitud))
                .ForMember(dest => dest.Ubicacion, opt => opt.MapFrom(src =>
                     src.SalaId.HasValue ? $"Sala {src.Sala.Numero}" : "Sin ubicación especificada"));
        }
        private void ReporteMapper()
        {
            // Mapeo para Registrar (ya lo usabas implícitamente, pero es bueno tenerlo)
            CreateMap<CrearReporteModel, Reporte>();

            // Mapeo para el Index "Mis Reportes"
            CreateMap<Reporte, ReporteIndexModel>()
                .ForMember(dest => dest.ObjetoAfectado, opt => opt.MapFrom(src =>
                    src.Tipo == TipoReporte.Sala
                    ? $"Sala {src.SalaReportada.Numero}"
                    : $"Equipo {src.EquipoReportado.Serial}"));
        }
        private void ReservaMapper()
        {
            CreateMap<Reserva, ReservaIndexModel>()
                .ForMember(dest => dest.ObjetoReservado, opt => opt.MapFrom(src =>
                    src.Tipo == TipoReserva.Sala
                    ? $"Sala {src.Sala.Numero}"
                    : $"Serial {src.Equipo.Serial}"))
                    .ForMember(dest => dest.SalaId, opt => opt.MapFrom(src => src.SalaId))
                    .ForMember(dest => dest.EquipoId, opt => opt.MapFrom(src => src.EquipoId));//
        }
        private void UsuarioMapper()
        {
            // Para el formulario de Registro
            CreateMap<RegistrarUsuarioModel, AppUser>();

            // Para la tabla del Index
            CreateMap<AppUser, UsuarioIndexModel>()
                .ForMember(dest => dest.NombreCompleto, opt => opt.MapFrom(src => $"{src.Name} {src.LastName}"))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Documento, opt => opt.MapFrom(src => src.DocumentNumber));
        }

        private void EquipoMapper()
        {
            // Mapea el modelo de registro a la entidad
            CreateMap<RegistrarEquipoModel, Equipo>();
            // Para el Index (GET)
            CreateMap<Equipo, EquipoIndexModel>()
                // Mapea el enum del Dominio al enum del Modelo
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => (EstadoEquipoModel)src.Estado))
                // Mapea el nombre de la sala
                .ForMember(dest => dest.SalaNombre, opt => opt.MapFrom(src => $"Sala {src.Sala.Numero}"));

            // Para Editar (GET)
            CreateMap<Equipo, EditarEquipoModel>()
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => (EstadoEquipoModel)src.Estado));

            // Para Editar (POST)
            CreateMap<EditarEquipoModel, Equipo>()
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => (EstadoEquipo)src.Estado));
        }
        private void MilkMapper()
        {
            CreateMap<Milk, MilkModel>()
            .ReverseMap();

        }

        private void CowMapper()
        {
            CreateMap<Cow, CowModel>()
            .ReverseMap();

            CreateMap<Cow, AddCowModel>()
            .ReverseMap();

        }

        private void FarmMapper()
        {
            CreateMap<Farm, FarmModel>()
                .ForMember(dest => dest.CowCount,
                           opt => opt.MapFrom(src => src.Cows != null ? src.Cows.Count : 0))
                .ForMember(dest => dest.TotalMilkLitters,opt => opt.MapFrom(src => src.getTotalLitters()))
            .ReverseMap();

            CreateMap<Farm, AddFarmModel>()
                .ReverseMap();
        }
        private void SalaMapper()
        {
            // 1. Mapeo para Registrar
            CreateMap<RegistrarSalaModel, Sala>();

            // 2. Mapeo para Editar (GET)
            CreateMap<Sala, EditarSalaModel>()
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => (EstadoSala)src.Estado));

            // 3. Mapeo para Editar (POST)
            CreateMap<EditarSalaModel, Sala>()
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => (EstadoSala)src.Estado));

            // 4. Mapeo para el Index
            CreateMap<Sala, SalaIndexModel>()
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => (EstadoSala)src.Estado));
        }

    }

    
}
