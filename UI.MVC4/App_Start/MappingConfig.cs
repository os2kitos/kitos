
using System.Linq;
using AutoMapper;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using UI.MVC4.Models;

[assembly: WebActivator.PreApplicationStartMethod(typeof(UI.MVC4.App_Start.MappingConfig), "Start")]

namespace UI.MVC4.App_Start
{
    public class MappingConfig
    {
         public static void Start()
         {
             Mapper.Initialize(cfg =>
                 {
                     cfg.AddProfile(new MappingProfile());
                     cfg.AddProfile(new DropdownProfile());
                 });
         }
    }

    public class DropdownProfile : Profile
    {
        protected override void Configure()
        {
            base.Configure();

            // TODO do we need an admin DTO and normal DTO to strip unused properties in normal DTO
            // like IsActive and Note

            Mapper.CreateMap<ContractTemplate, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.UseValue(null));

            Mapper.CreateMap<ContractType, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.UseValue(null));

            Mapper.CreateMap<DatabaseType, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.UseValue(null));

            Mapper.CreateMap<Environment, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.UseValue(null));

            Mapper.CreateMap<InterfaceType, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.UseValue(null));

            Mapper.CreateMap<Method, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.UseValue(null));

            Mapper.CreateMap<PaymentModel, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.UseValue(null));

            Mapper.CreateMap<ProjectCategory, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.UseValue(null));

            Mapper.CreateMap<ProjectType, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.UseValue(null));

            Mapper.CreateMap<ProtocolType, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.UseValue(null));

            Mapper.CreateMap<PurchaseForm, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.UseValue(null));

            Mapper.CreateMap<SystemType, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.UseValue(null));

            Mapper.CreateMap<Config, ConfigDTO>()
                  .ForMember(dest => dest.ItContractName, opt => opt.MapFrom(src => src.ItContractModuleName.Name))
                  .ForMember(dest => dest.ItProjectName, opt => opt.MapFrom(src => src.ItProjectModuleName.Name))
                  .ForMember(dest => dest.ItSystemName, opt => opt.MapFrom(src => src.ItSystemModuleName.Name))
                  .ForMember(dest => dest.ItSupportName, opt => opt.MapFrom(src => src.ItSupportModuleName.Name))
                  .ReverseMap();
        }
    }

    public class MappingProfile : Profile
    {
        protected override void Configure()
        {
            base.Configure();

            Mapper.CreateMap<User, UserApiModel>().ReverseMap();

            Mapper.CreateMap<Municipality, MunicipalityApiModel>().ReverseMap();
        }
    }
}