using AutoMapper;
using Domain;
using Domain.Enums;
using Services.Models.CowModels;
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
