
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

            Mapper.CreateMap<ContractTemplate, ContractTemplateDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.ItContracts, opt => opt.UseValue(null));

            Mapper.CreateMap<ContractType, ContractTypeDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.ItContracts, opt => opt.UseValue(null));

            Mapper.CreateMap<DatabaseType, DatabaseTypeDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.Technologies, opt => opt.UseValue(null));

            Mapper.CreateMap<Environment, EnvironmentDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.Technologies, opt => opt.UseValue(null));

            Mapper.CreateMap<InterfaceType, InterfaceTypeDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.ItSystems, opt => opt.UseValue(null));

            Mapper.CreateMap<Method, MethodDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.Interfaces, opt => opt.UseValue(null));

            Mapper.CreateMap<PaymentModel, PaymentModelDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.ItContracts, opt => opt.UseValue(null));

            Mapper.CreateMap<ProjectCategory, ProjectCategoryDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.ItProjects, opt => opt.UseValue(null));

            Mapper.CreateMap<ProjectType, ProjectTypeDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.ItProjects, opt => opt.UseValue(null));

            Mapper.CreateMap<ProtocolType, ProtocolTypeDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.ItSystems, opt => opt.UseValue(null));

            Mapper.CreateMap<PurchaseForm, PurchaseFormDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.ItContracts, opt => opt.UseValue(null));

            Mapper.CreateMap<SystemType, SystemTypeDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.ItSystem, opt => opt.UseValue(null));
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