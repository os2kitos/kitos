using System.Linq;
using AutoMapper;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
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
                  .ForMember(dest => dest.References, opt => opt.Ignore());

            Mapper.CreateMap<ArchiveType, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore());

            Mapper.CreateMap<ContractTemplate, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore());

            Mapper.CreateMap<ContractType, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore());

            Mapper.CreateMap<InterfaceType, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore());

            Mapper.CreateMap<Interface, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore());

            Mapper.CreateMap<DataType, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore());

            Mapper.CreateMap<Method, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore());

            Mapper.CreateMap<ItProjectType, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore()); 

            Mapper.CreateMap<PurchaseForm, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore());

            Mapper.CreateMap<Tsa, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore());

            Mapper.CreateMap<Frequency, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore());

            Mapper.CreateMap<BusinessType, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore());

            Mapper.CreateMap<ItProjectRole, RoleDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore());

            Mapper.CreateMap<ItSystemRole, RoleDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore());

            Mapper.CreateMap<ItContractRole, RoleDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore());

            Mapper.CreateMap<ItSystemTypeOption, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore());

            Mapper.CreateMap<OrganizationRole, RoleDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore());

            Mapper.CreateMap<AdminRole, RoleDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore());

            Mapper.CreateMap<SensitiveDataType, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore());

            Mapper.CreateMap<GoalType, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore());

            Mapper.CreateMap<ProcurementStrategy, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore());

            Mapper.CreateMap<OptionExtend, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore());

            Mapper.CreateMap<PriceRegulation, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore());

            Mapper.CreateMap<PaymentModel, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore());

            Mapper.CreateMap<PaymentFreqency, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore());

            Mapper.CreateMap<TerminationDeadline, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore());

            Mapper.CreateMap<HandoverTrialType, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore());

            Mapper.CreateMap<Config, ConfigDTO>().ReverseMap();
        }
    }

    public class MappingProfile : Profile
    {
        protected override void Configure()
        {
            base.Configure();

            Mapper.CreateMap<Text, TextDTO>()
                  .ReverseMap();

            Mapper.CreateMap<User, UserDTO>()
                  .ReverseMap();
            Mapper.CreateMap<User, UserProfileDTO>()
                  .ReverseMap();

            Mapper.CreateMap<Wish, WishDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.User, opt => opt.Ignore())
                  .ForMember(dest => dest.ItSystemUsage, opt => opt.Ignore());

            Mapper.CreateMap<Organization, OrganizationDTO>()
                .ForMember(dest => dest.Root, opt => opt.MapFrom(src => src.GetRoot()))
                .ReverseMap();

            Mapper.CreateMap<OrganizationUnit, OrgUnitDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.Children, opt => opt.Ignore());

            Mapper.CreateMap<OrganizationUnit, OrgUnitSimpleDTO>();
            Mapper.CreateMap<OrganizationUnit, SimpleOrgUnitDTO>();

            Mapper.CreateMap<PasswordResetRequest, PasswordResetRequestDTO>()
                  .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email));

            Mapper.CreateMap<TaskRef, TaskRefDTO>()
                  .ReverseMap();

            Mapper.CreateMap<TaskUsage, TaskUsageDTO>()
                .ForMember(dto => dto.HasDelegations, opt => opt.MapFrom(src => src.Children.Any()))
                .ReverseMap();

            Mapper.CreateMap<TaskUsage, TaskUsageNestedDTO>()
                .ForMember(dto => dto.HasDelegations, opt => opt.MapFrom(src => src.Children.Any()))
                .ReverseMap();

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

            Mapper.CreateMap<ItContractRight, RightOutputDTO>();
            Mapper.CreateMap<RightInputDTO, ItContractRight>();

            Mapper.CreateMap<ItInterfaceExhibit, ItInterfaceExhibitDTO>()
                .ReverseMap();

            Mapper.CreateMap<ItInterfaceExhibitUsage, ItInterfaceExhibitUsageDTO>()
                .ReverseMap();

            Mapper.CreateMap<DataRow, DataRowDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.DataType, opt => opt.Ignore());

            Mapper.CreateMap<ItSystem, ItSystemDTO>()
                .ForMember(dest => dest.TaskRefIds, opt => opt.MapFrom(src => src.TaskRefs.Select(x => x.Id)))
                .ForMember(dest => dest.IsUsed, opt => opt.MapFrom(src => src.Usages.Any()))
                //.ForMember(dest => dest.CanUseInterfaceIds, opt => opt.MapFrom(src => src.CanUseInterfaces.Select(x => x.Id)))
                //.ForMember(dest => dest.ExposedInterfaceIds, opt => opt.MapFrom(src => src.ItInterfaceExhibits.Select(x => x.Id)))
                .ReverseMap()
                .ForMember(dest => dest.TaskRefs, opt => opt.Ignore())
                .ForMember(dest => dest.CanUseInterfaces, opt => opt.Ignore())
                .ForMember(dest => dest.ItInterfaceExhibits, opt => opt.Ignore());
                  //.ForMember(dest => dest.CanBeUsedBy, opt => opt.Ignore());

            //Simplere mapping than the one above, only one way
            Mapper.CreateMap<ItSystem, ItSystemSimpleDTO>();

            Mapper.CreateMap<ItInterface, ItInterfaceDTO>()
                .ForMember(dest => dest.InterfaceTypeName, opt => opt.MapFrom(src => src.InterfaceType.Name)) // have to map here else the EF proxy class type name is used :(
                .ForMember(dest => dest.IsUsed, opt => opt.MapFrom(src => src.ExhibitedBy.ItSystem.Usages.Any()))
                .ReverseMap();

            Mapper.CreateMap<ItInterfaceUse, ItInterfaceUseDTO>()
                  .ReverseMap();

            Mapper.CreateMap<DataRowUsage, DataRowUsageDTO>()
                  .ReverseMap();

            Mapper.CreateMap<InterfaceUsage, InterfaceUsageDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.ItContract, opt => opt.Ignore());

            Mapper.CreateMap<ItInterfaceExhibitUsage, InterfaceExposureDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.ItContract, opt => opt.Ignore());

            Mapper.CreateMap<ItSystemUsage, ItSystemUsageDTO>()
                .ForMember(dest => dest.ResponsibleOrgUnitName,
                    opt => opt.MapFrom(src => src.ResponsibleUsage.OrganizationUnit.Name))
                .ForMember(dest => dest.Contracts, opt => opt.MapFrom(src => src.Contracts.Select(x => x.ItContract)))
                .ForMember(dest => dest.MainContractId, opt => opt.MapFrom(src => src.MainContract.ItContractId))
                .ForMember(dest => dest.MainContractIsActive, opt => opt.MapFrom(src => src.MainContract.ItContract.IsActive))
                .ForMember(dest => dest.InterfaceExhibitCount, opt => opt.MapFrom(src => src.ItSystem.ItInterfaceExhibits.Count))
                .ForMember(dest => dest.InterfaceUseCount, opt => opt.MapFrom(src => src.ItSystem.CanUseInterfaces.Count))
                .ForMember(dest => dest.ActiveInterfaceUseCount, opt => opt.MapFrom(src => src.ItSystem.CanUseInterfaces.Sum(x => x.InterfaceUsages.Count(y => y.ItContract.IsActive))))
                .ReverseMap()
                .ForMember(dest => dest.OrgUnits, opt => opt.Ignore())
                .ForMember(dest => dest.TaskRefs, opt => opt.Ignore())
                .ForMember(dest => dest.ItProjects, opt => opt.Ignore())
                .ForMember(dest => dest.Contracts, opt => opt.Ignore());

            //Simplere mapping than the one above, only one way
            Mapper.CreateMap<ItSystemUsage, ItSystemUsageSimpleDTO>();

            Mapper.CreateMap<EconomyYear, EconomyYearDTO>()
                  .ReverseMap();

            Mapper.CreateMap<Risk, RiskDTO>()
                .ReverseMap()
                .ForMember(dest => dest.ItProject, opt => opt.Ignore())
                .ForMember(dest => dest.ResponsibleUser, opt => opt.Ignore());

            Mapper.CreateMap<Stakeholder, StakeholderDTO>()
                .ReverseMap()
                .ForMember(dest => dest.ItProject, opt => opt.Ignore());

            Mapper.CreateMap<ItProjectStatus, ItProjectStatusDTO>()
                  .Include<Assignment, AssignmentDTO>()
                  .Include<Milestone, MilestoneDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.AssociatedUser, opt => opt.Ignore())
                  .ForMember(dest => dest.ObjectOwner, opt => opt.Ignore())
                  .ForMember(dest => dest.ObjectOwnerId, opt => opt.Ignore());

            Mapper.CreateMap<Assignment, AssignmentDTO>()
                  .ReverseMap();
            Mapper.CreateMap<Milestone, MilestoneDTO>()
                  .ReverseMap();
                
            Mapper.CreateMap<GoalStatus, GoalStatusDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.ItProject, opt => opt.Ignore())
                  .ForMember(dest => dest.Goals, opt => opt.Ignore());

            Mapper.CreateMap<Goal, GoalDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.GoalStatus, opt => opt.Ignore())
                  .ForMember(dest => dest.GoalType, opt => opt.Ignore());

            Mapper.CreateMap<Stakeholder, StakeholderDTO>()
                  .ReverseMap();

            

            Mapper.CreateMap<ItProject, ItProjectDTO>()
                .ForMember(dest => dest.ChildrenIds,
                    opt => opt.MapFrom(x => x.Children.Select(y => y.Id)))
                .ReverseMap()
                .ForMember(dest => dest.Children, opt => opt.Ignore())
                .ForMember(dest => dest.ItSystemUsages, opt => opt.Ignore())
                .ForMember(dest => dest.TaskRefs, opt => opt.Ignore())
                .ForMember(dest => dest.UsedByOrgUnits, opt => opt.Ignore())
                .ForMember(dest => dest.Stakeholders, opt => opt.Ignore());

            Mapper.CreateMap<ItProjectPhase, ItProjectPhaseDTO>()
                  .ReverseMap();

            //Output only - this mapping should not be reversed
            Mapper.CreateMap<ItProject, ItProjectCatalogDTO>();

            //Output only - this mapping should not be reversed
            Mapper.CreateMap<ItProject, ItProjectSimpleDTO>();

            //Output only - this mapping should not be reversed
            Mapper.CreateMap<ItProject, ItProjectOverviewDTO>()
                .ForMember(dest => dest.ResponsibleOrgUnitName, opt => opt.MapFrom(src => src.ResponsibleUsage.OrganizationUnit.Name));

            Mapper.CreateMap<Handover, HandoverDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.ItProject, opt => opt.Ignore());

            Mapper.CreateMap<Communication, CommunicationDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.ItProject, opt => opt.Ignore());

            Mapper.CreateMap<ItContract, ItContractDTO>()
                  .ForMember(dest => dest.AssociatedSystemUsages, opt => opt.MapFrom(src => src.AssociatedSystemUsages.Select(x => x.ItSystemUsage)))
                  .ReverseMap()
                  .ForMember(contract => contract.AssociatedSystemUsages, opt => opt.Ignore())
                  .ForMember(contract => contract.AssociatedInterfaceExposures, opt => opt.Ignore())
                  .ForMember(contract => contract.AssociatedInterfaceUsages, opt => opt.Ignore())
                  .ForMember(contract => contract.InternEconomyStreams, opt => opt.Ignore())
                  .ForMember(contract => contract.ExternEconomyStreams, opt => opt.Ignore());

            //Output only - this mapping should not be reversed
            Mapper.CreateMap<ItContract, ItContractOverviewDTO>();

            //Output only - this mapping should not be reversed
            Mapper.CreateMap<ItContract, ItContractPlanDTO>();

            //Output only - this mapping should not be reversed
            Mapper.CreateMap<ItContract, ItContractSystemDTO>(); 

            Mapper.CreateMap<PaymentMilestone, PaymentMilestoneDTO>()
                  .ReverseMap();

            Mapper.CreateMap<EconomyStream, EconomyStreamDTO>()
                .ReverseMap();

            Mapper.CreateMap<Advice, AdviceDTO>()
                  .ReverseMap();

            Mapper.CreateMap<HandoverTrial, HandoverTrialDTO>()
                  .ReverseMap();
        }
    }
}
