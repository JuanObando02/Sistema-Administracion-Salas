using AutoMapper;
using Domain;
using Domain.Enums;
using Services.Models.CowModels;
using Services.Models.EquipoModels;
using Services.Models.FarmModels;
using Services.Models.MilkModels;
using Services.Models.SalaModels;

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
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => (EstadoSalaModel)src.Estado));

            // 3. Mapeo para Editar (POST)
            CreateMap<EditarSalaModel, Sala>()
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => (EstadoSalaModel)src.Estado));

            // 4. Mapeo para el Index
            CreateMap<Sala, SalaIndexModel>()
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => (EstadoSalaModel)src.Estado));
        }

    }

    
}
