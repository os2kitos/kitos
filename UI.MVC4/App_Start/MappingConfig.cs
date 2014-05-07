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

            Mapper.CreateMap<AgreementElement, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.UseValue(null));

            Mapper.CreateMap<ArchiveType, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.UseValue(null));

            Mapper.CreateMap<HandoverTrial, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.UseValue(null));

            Mapper.CreateMap<ContractTemplate, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.UseValue(null));

            Mapper.CreateMap<ContractType, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.UseValue(null));

            Mapper.CreateMap<InterfaceType, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.UseValue(null));

            Mapper.CreateMap<InterfaceCategory, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.UseValue(null));

            Mapper.CreateMap<Interface, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.UseValue(null));

            Mapper.CreateMap<DataType, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.UseValue(null));

            Mapper.CreateMap<Method, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.UseValue(null));

            Mapper.CreateMap<PaymentModel, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.UseValue(null));

            Mapper.CreateMap<ItProjectCategory, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.UseValue(null));

            Mapper.CreateMap<ProjectPhase, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.UseValue(null));

            Mapper.CreateMap<ItProjectType, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.UseValue(null));

            Mapper.CreateMap<PurchaseForm, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.UseValue(null));

            Mapper.CreateMap<Tsa, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.UseValue(null));

            Mapper.CreateMap<Frequency, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.UseValue(null));

            Mapper.CreateMap<AppType, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.UseValue(null));

            Mapper.CreateMap<BusinessType, OptionDTO>()
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

            Mapper.CreateMap<OrganizationRole, RoleDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.UseValue(null));

            Mapper.CreateMap<AdminRole, RoleDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.UseValue(null));

            Mapper.CreateMap<SensitiveDataType, OptionDTO>()
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
                  .ReverseMap();
            Mapper.CreateMap<User, UserProfileDTO>()
                  .ReverseMap();

            Mapper.CreateMap<Wish, WishDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.User, opt => opt.Ignore())
                  .ForMember(dest => dest.ItSystemUsage, opt => opt.Ignore());

            Mapper.CreateMap<Organization, OrganizationDTO>().ReverseMap();
            Mapper.CreateMap<OrganizationUnit, OrgUnitDTO>().ReverseMap();

            Mapper.CreateMap<PasswordResetRequest, PasswordResetRequestDTO>()
                  .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email));

            Mapper.CreateMap<TaskRef, TaskRefDTO>()
                  .ReverseMap();

            Mapper.CreateMap<TaskUsage, TaskUsageDTO>().ReverseMap();

            Mapper.CreateMap<AdminRight, AdminRightDTO>()
                  .ForMember(dto => dto.OrganizationId, opt => opt.MapFrom(src => src.ObjectId))
                  .ForMember(dto => dto.RoleName, opt => opt.MapFrom(src => src.Role.Name));

            Mapper.CreateMap<OrganizationRight, RightOutputDTO>();
            Mapper.CreateMap<RightInputDTO, OrganizationRight>();

            Mapper.CreateMap<AdminRight, RightOutputDTO>();
            Mapper.CreateMap<RightInputDTO, AdminRight>();

            Mapper.CreateMap<ItSystemRight, RightOutputDTO>();
            Mapper.CreateMap<RightInputDTO, ItSystemRight>();

            Mapper.CreateMap<ItProjectRight, RightOutputDTO>();
            Mapper.CreateMap<RightInputDTO, ItProjectRight>();

            Mapper.CreateMap<DataRow, DataRowDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.DataType, opt => opt.Ignore());

            Mapper.CreateMap<ItSystem, ItSystemDTO>()
                  .ForMember(dest => dest.TaskRefIds, opt => opt.MapFrom(src => src.TaskRefs.Select(x => x.Id)))
                  .ForMember(dest => dest.CanUseInterfaceIds, opt => opt.MapFrom(src => src.CanUseInterfaces.Select(x => x.Id)))
                  .ForMember(dest => dest.ExposedInterfaceIds, opt => opt.MapFrom(src => src.ExposedInterfaces.Select(x => x.Id)))
                  .ForMember(dest => dest.CanBeUsedByIds, opt => opt.MapFrom(src => src.CanBeUsedBy.Select(x => x.Id)))
                  .ReverseMap()
                  .ForMember(dest => dest.TaskRefs, opt => opt.Ignore())
                  .ForMember(dest => dest.CanUseInterfaces, opt => opt.Ignore())
                  .ForMember(dest => dest.ExposedInterfaces, opt => opt.Ignore())
                  .ForMember(dest => dest.CanBeUsedBy, opt => opt.Ignore());

            Mapper.CreateMap<DataRowUsage, DataRowUsageDTO>()
                  .ReverseMap();

            Mapper.CreateMap<InterfaceUsage, InterfaceUsageDTO>()
                  .ReverseMap();

            Mapper.CreateMap<ItSystemUsage, ItSystemUsageDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.OrgUnits, opt => opt.Ignore())
                  .ForMember(dest => dest.TaskRefs, opt => opt.Ignore())
                  .ForMember(dest => dest.ItProjects, opt => opt.Ignore());

            Mapper.CreateMap<EconomyYear, EconomyYearDTO>()
                  .ReverseMap();

            Mapper.CreateMap<Activity, ActivityDTO>()
                .ForMember(dest => dest.AssociatedUser, opt => opt.Ignore())
                .ForMember(dest => dest.ObjectOwner, opt => opt.Ignore());

            Mapper.CreateMap<ItProject, ItProjectDTO>()
                .ForMember(dest => dest.AssociatedProjectIds,
                    opt => opt.MapFrom(x => x.AssociatedProjects.Select(y => y.Id)))
                .ForMember(dest => dest.ItSystems, opt => opt.MapFrom(src => src.ItSystemUsages.Select(x => x.ItSystem)))
                .ReverseMap()
                .ForMember(dest => dest.AssociatedProjects, opt => opt.Ignore())
                .ForMember(dest => dest.ItSystemUsages, opt => opt.Ignore())
                .ForMember(dest => dest.TaskRefs, opt => opt.Ignore());
            //.ForMember(dest => dest.Phases, opt => opt.Ignore());
        }
    }
}