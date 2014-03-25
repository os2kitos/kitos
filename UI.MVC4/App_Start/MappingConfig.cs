
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

            Mapper.CreateMap<ProjectPhase, OptionDTO>()
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

            Mapper.CreateMap<ItContractModuleName, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.UseValue(null));

            Mapper.CreateMap<ItProjectModuleName, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.UseValue(null));

            Mapper.CreateMap<ItSupportModuleName, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.UseValue(null));

            Mapper.CreateMap<ItSystemModuleName, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.UseValue(null));

            Mapper.CreateMap<ExtReferenceType, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.UseValue(null));

            Mapper.CreateMap<ItProjectRole, RoleDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.UseValue(null));

            Mapper.CreateMap<ItSystemRole, RoleDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.UseValue(null));

            Mapper.CreateMap<ItContractRole, RoleDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.UseValue(null));

            Mapper.CreateMap<DepartmentRole, RoleDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.UseValue(null));

            Mapper.CreateMap<ProjPhaseLocale, LocaleDTO>().ReverseMap();
            Mapper.CreateMap<ExtRefTypeLocale, LocaleDTO>().ReverseMap();

            Mapper.CreateMap<ProjPhaseLocale, LocaleInputDTO>().ReverseMap();
            Mapper.CreateMap<ExtRefTypeLocale, LocaleInputDTO>().ReverseMap();

            Mapper.CreateMap<Config, ConfigDTO>().ReverseMap();
            
        }
    }

    public class MappingProfile : Profile
    {
        protected override void Configure()
        {
            base.Configure();

            Mapper.CreateMap<User, UserDTO>()
                .ForMember(dto => dto.RoleName, opt => opt.MapFrom(src => src.Role != null ? src.Role.Name : ""))
                .ReverseMap();

            Mapper.CreateMap<Municipality, MunicipalityApiModel>().ReverseMap();

            Mapper.CreateMap<PasswordResetRequest, PasswordResetRequestDTO>()
                  .ForMember(dto => dto.UserEmail, opt => opt.MapFrom(src => src.User.Email));

            Mapper.CreateMap<TaskRef, TaskRefDTO>()
                  .ReverseMap();
        }
    }
}