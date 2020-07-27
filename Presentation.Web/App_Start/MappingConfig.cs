using System.Linq;
using AutoMapper;
using Core.ApplicationServices;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Presentation.Web.Models;
using Core.DomainModel.Advice;
using Core.DomainModel.ItSystemUsage.GDPR;
using Presentation.Web.Models.ItSystemUsage;
using Advice = Core.DomainModel.Advice.Advice;
using ContactPerson = Core.DomainModel.ContactPerson;
using DataRow = Core.DomainModel.ItSystem.DataRow;
using ExternalReference = Core.DomainModel.ExternalReference;
using Organization = Core.DomainModel.Organization.Organization;
using Text = Core.DomainModel.Text;

namespace Presentation.Web
{
    public class MappingConfig
    {
        public static IMapper CreateMapper()
        {
            var mappingconfig = new MapperConfiguration(configure: cfg =>
            {
                cfg.AddProfile<MappingProfile>();
                cfg.AddProfile<DropdownProfile>();
            });

            return mappingconfig.CreateMapper();
        }
    }

    public class DropdownProfile : Profile
    {
        public DropdownProfile()
        {

            CreateMap<ContactPerson, ContactPersonDTO>()
                 .ReverseMap();


            CreateMap<ExternalReference, ExternalReferenceDTO>()
                  .ReverseMap();

            CreateMap<ItSystemUsageDataWorkerRelation, ItSystemUsageDataWorkerRelationDTO>()
                  .ForMember(dest => dest.DataWorkerName, opt => opt.MapFrom(src => src.DataWorker.Name))
                  .ForMember(dest => dest.DataWorkerCvr, opt => opt.MapFrom(src => src.DataWorker.Cvr))
                  .ReverseMap();

            CreateMap<AgreementElementType, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore());

            CreateMap<ArchiveType, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore());

            CreateMap<ItContractTemplateType, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore());

            CreateMap<ItContractType, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore());

            CreateMap<InterfaceType, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore());

            CreateMap<DataType, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore());

            CreateMap<ItProjectType, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore());

            CreateMap<PurchaseFormType, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore());

            CreateMap<RelationFrequencyType, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore());

            CreateMap<BusinessType, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore());

            CreateMap<ItProjectRole, RoleDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore());

            CreateMap<ItSystemRole, RoleDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore());

            CreateMap<ItContractRole, RoleDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore());

            CreateMap<OrganizationUnitRole, RoleDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore());

            CreateMap<SensitiveDataType, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore());

            CreateMap<GoalType, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore());

            CreateMap<ProcurementStrategyType, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore());

            CreateMap<OptionExtendType, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore());

            CreateMap<PriceRegulationType, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore());

            CreateMap<PaymentModelType, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore());

            CreateMap<PaymentFreqencyType, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore());

            CreateMap<TerminationDeadlineType, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore());

            CreateMap<HandoverTrialType, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore());

            CreateMap<Config, ConfigDTO>().ReverseMap();

            CreateMap<DataProtectionAdvisor, DataProtectionAdvisorDTO>()
                  .ReverseMap();

            CreateMap<DataResponsible, DataResponsibleDTO>()
                  .ReverseMap();
        }
    }

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Text, TextDTO>()
                  .ReverseMap();

            CreateMap<User, UserDTO>()
                  .ForMember(dest => dest.DefaultOrganizationUnitId,
                      opt => opt.MapFrom(src => src.OrganizationRights.FirstOrDefault() != null ? src.OrganizationRights.First().DefaultOrgUnitId : null))
                  .ForMember(dest => dest.DefaultOrganizationUnitName,
                      opt => opt.MapFrom(src => src.OrganizationRights.FirstOrDefault() != null ? src.OrganizationRights.First().DefaultOrgUnit.Name : null))
                  .ReverseMap();

            CreateMap<User, UserOverviewDTO>()
                .ForMember(dest => dest.DefaultOrganizationUnitId,
                      opt => opt.MapFrom(src => src.OrganizationRights.FirstOrDefault() != null ? src.OrganizationRights.First().DefaultOrgUnitId : null))
                  .ForMember(dest => dest.DefaultOrganizationUnitName,
                      opt => opt.MapFrom(src => src.OrganizationRights.FirstOrDefault() != null ? src.OrganizationRights.First().DefaultOrgUnit.Name : null));

            CreateMap<Organization, OrganizationDTO>()
                .ForMember(dest => dest.Root, opt => opt.MapFrom(src => src.GetRoot()))
                .ReverseMap();

            CreateMap<Organization, OrganizationSimpleDTO>();

            CreateMap<OrganizationUnit, OrgUnitDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.Children, opt => opt.Ignore());

            CreateMap<OrganizationUnit, OrgUnitSimpleDTO>();
            CreateMap<OrganizationUnit, SimpleOrgUnitDTO>();

            CreateMap<PasswordResetRequest, PasswordResetRequestDTO>()
                  .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email));

            CreateMap<TaskRef, TaskRefDTO>()
                  .ReverseMap();

            CreateMap<TaskUsage, TaskUsageDTO>()
                .ForMember(dto => dto.HasDelegations, opt => opt.MapFrom(src => src.Children.Any()))
                .ReverseMap();

            CreateMap<TaskUsage, TaskUsageNestedDTO>()
                .ForMember(dto => dto.HasDelegations, opt => opt.MapFrom(src => src.Children.Any()))
                .ReverseMap();

            CreateMap<OrganizationRight, OrganizationRightDTO>()
                .ReverseMap();

            CreateMap<OrganizationUnitRight, RightOutputDTO>();
            CreateMap<RightInputDTO, OrganizationUnitRight>();

            CreateMap<ItSystemRight, RightOutputDTO>()
                .ForMember(dto => dto.ObjectName, opt => opt.MapFrom(src => src.Object.ItSystem.Name));
            CreateMap<RightInputDTO, ItSystemRight>();

            CreateMap<ItSystemRight, ReportItSystemRightOutputDTO>()
               .ForMember(dto => dto.roleId, opt => opt.MapFrom(src => src.Role.Id))
               .ForMember(dto => dto.roleName, opt => opt.MapFrom(src => src.Role.Name))
               .ForMember(dto => dto.roleisSuggestion, opt => opt.MapFrom(src => src.Role.IsSuggestion))
               .ForMember(dto => dto.roleHasreadAccess, opt => opt.MapFrom(src => src.Role.HasReadAccess))
               .ForMember(dto => dto.roleHasWriteAccess, opt => opt.MapFrom(src => src.Role.HasWriteAccess))
               .ForMember(dto => dto.roleHasreadAccess, opt => opt.MapFrom(src => src.Role.HasReadAccess))
               .ForMember(dto => dto.roleObjectOwnerId, opt => opt.MapFrom(src => src.Role.ObjectOwnerId))
               .ForMember(dto => dto.roleLastChanged, opt => opt.MapFrom(src => src.Role.LastChanged))
               .ForMember(dto => dto.roleLastChangedByUserId, opt => opt.MapFrom(src => src.Role.LastChangedByUserId))
               .ForMember(dto => dto.roleDescription, opt => opt.MapFrom(src => src.Role.Description))
               .ForMember(dto => dto.roleIsObligatory, opt => opt.MapFrom(src => src.Role.IsObligatory))
               .ForMember(dto => dto.roleIsEnabled, opt => opt.MapFrom(src => src.Role.IsEnabled))
               .ForMember(dto => dto.roleIsLocallyAvailable, opt => opt.MapFrom(src => src.Role.IsLocallyAvailable))
               .ForMember(dto => dto.rolePriority, opt => opt.MapFrom(src => src.Role.Priority))
               .ForMember(dto => dto.itSystemRightId, opt => opt.MapFrom(src => src.Id))
               .ForMember(dto => dto.itSystemRightUserId, opt => opt.MapFrom(src => src.UserId))
               .ForMember(dto => dto.itSystemRightRoleId, opt => opt.MapFrom(src => src.RoleId))
               .ForMember(dto => dto.itSystemRightObjectId, opt => opt.MapFrom(src => src.ObjectId))
               .ForMember(dto => dto.itSystemRightObjectOwnerId, opt => opt.MapFrom(src => src.ObjectOwnerId))
               .ForMember(dto => dto.itSystemRightLastChanged, opt => opt.MapFrom(src => src.LastChanged))
               .ForMember(dto => dto.itSystemRightLastChangedByUserId, opt => opt.MapFrom(src => src.LastChangedByUserId))
               .ForMember(dto => dto.userId, opt => opt.MapFrom(src => src.UserId))
               .ForMember(dto => dto.userName, opt => opt.MapFrom(src => src.User.Name))
               .ForMember(dto => dto.userLastName, opt => opt.MapFrom(src => src.User.LastName))
               .ForMember(dto => dto.userPhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
               .ForMember(dto => dto.userEmail, opt => opt.MapFrom(src => src.User.Email))
               .ForMember(dto => dto.userPassword, opt => opt.MapFrom(src => src.User.Password))
               .ForMember(dto => dto.userSalt, opt => opt.MapFrom(src => src.User.Salt))
               .ForMember(dto => dto.userIsGlobalAdmin, opt => opt.MapFrom(src => src.User.IsGlobalAdmin))
               .ForMember(dto => dto.userLastAdvisDate, opt => opt.MapFrom(src => src.User.LastAdvisDate))
               .ForMember(dto => dto.userObjectOwnerId, opt => opt.MapFrom(src => src.User.ObjectOwnerId))
               .ForMember(dto => dto.userLastChanged, opt => opt.MapFrom(src => src.User.LastChanged))
               .ForMember(dto => dto.userLastChangedByUserId, opt => opt.MapFrom(src => src.User.LastChangedByUserId))
               .ForMember(dto => dto.userDefaultOrganizationId, opt => opt.MapFrom(src => src.User.DefaultOrganizationId))
               .ForMember(dto => dto.userLockedOutDate, opt => opt.MapFrom(src => src.User.LockedOutDate))
               .ForMember(dto => dto.userFailedAttempts, opt => opt.MapFrom(src => src.User.FailedAttempts))
               .ForMember(dto => dto.userDefaultUserStartPreference, opt => opt.MapFrom(src => src.User.DefaultUserStartPreference))
               .ForMember(dto => dto.ItSystemUsageId, opt => opt.MapFrom(src => src.Object.Id))
               .ForMember(dto => dto.ItSystemUsageIsStatusActive, opt => opt.MapFrom(src => src.Object.IsStatusActive))
               .ForMember(dto => dto.ItSystemUsageNote, opt => opt.MapFrom(src => src.Object.Note))
               .ForMember(dto => dto.ItSystemUsageLocalSystemId, opt => opt.MapFrom(src => src.Object.LocalSystemId))
               .ForMember(dto => dto.ItSystemUsageVersion, opt => opt.MapFrom(src => src.Object.Version))
               .ForMember(dto => dto.ItSystemUsageEsdhRef, opt => opt.MapFrom(src => src.Object.EsdhRef))
               .ForMember(dto => dto.ItSystemUsageCmdbRef, opt => opt.MapFrom(src => src.Object.CmdbRef))
               .ForMember(dto => dto.ItSystemUsageDirectoryOrUrlRef, opt => opt.MapFrom(src => src.Object.DirectoryOrUrlRef))
               .ForMember(dto => dto.ItSystemUsageLocalCallName, opt => opt.MapFrom(src => src.Object.LocalCallName))
               .ForMember(dto => dto.ItSystemUsageOrganizationId, opt => opt.MapFrom(src => src.Object.OrganizationId))
               .ForMember(dto => dto.ItSystemUsageItSystemId, opt => opt.MapFrom(src => src.Object.ItSystemId))
               .ForMember(dto => dto.ItSystemUsageArchiveTypeId, opt => opt.MapFrom(src => src.Object.ArchiveTypeId))
               .ForMember(dto => dto.ItSystemUsageSensitiveDataTypeId, opt => opt.MapFrom(src => src.Object.SensitiveDataTypeId))
               .ForMember(dto => dto.ItSystemUsageOverviewId, opt => opt.MapFrom(src => src.Object.OverviewId))
               .ForMember(dto => dto.ItSystemUsageObjectOwnerId, opt => opt.MapFrom(src => src.Object.ObjectOwnerId))
               .ForMember(dto => dto.ItSystemUsageLastChanged, opt => opt.MapFrom(src => src.Object.LastChanged))
               .ForMember(dto => dto.ItSystemUsageLastChangedByUserId, opt => opt.MapFrom(src => src.Object.LastChangedByUserId))
               //.ForMember(dto => dto.ItSystemUsageOrganizationUnit_Id, opt => opt.MapFrom(src => src.Object.Organization))
               .ForMember(dto => dto.ItSystemUsageReferenceId, opt => opt.MapFrom(src => src.Object.ReferenceId))
               .ForMember(dto => dto.ItSystemUsageActive, opt => opt.MapFrom(src => src.Object.Active))
               .ForMember(dto => dto.ItSystemUsageConcluded, opt => opt.MapFrom(src => src.Object.Concluded))
               .ForMember(dto => dto.ItSystemUsageExpirationDate, opt => opt.MapFrom(src => src.Object.ExpirationDate))
               .ForMember(dto => dto.ItSystemUsageTerminated, opt => opt.MapFrom(src => src.Object.Terminated))
               .ForMember(dto => dto.ItSystemUsageTerminationDeadlineInSystem_Id, opt => opt.MapFrom(src => src.Object.TerminationDeadlineInSystem.Id))
               .ForMember(dto => dto.ItSystemUsageArchiveDuty, opt => opt.MapFrom(src => src.Object.ArchiveDuty))
               .ForMember(dto => dto.ItSystemUsageReportedToDPA, opt => opt.MapFrom(src => src.Object.ReportedToDPA))
               .ForMember(dto => dto.ItSystemUsageDocketNo, opt => opt.MapFrom(src => src.Object.DocketNo))
               .ForMember(dto => dto.ItSystemUsageArchivedDate, opt => opt.MapFrom(src => src.Object.ArchivedDate))
               .ForMember(dto => dto.ItSystemUsageArchiveNotes, opt => opt.MapFrom(src => src.Object.ArchiveNotes))
               .ForMember(dto => dto.ItSystemUsageArchiveLocationId, opt => opt.MapFrom(src => src.Object.ArchiveLocationId))
               .ForMember(dto => dto.OrganizationId, opt => opt.MapFrom(src => src.Object.Organization.Id))
               .ForMember(dto => dto.OrganizationName, opt => opt.MapFrom(src => src.Object.Organization.Name))
               .ForMember(dto => dto.OrganizationCvr, opt => opt.MapFrom(src => src.Object.Organization.Cvr))
               .ForMember(dto => dto.OrganizationAccessModifier, opt => opt.MapFrom(src => src.Object.Organization.AccessModifier))
               .ForMember(dto => dto.OrganizationUuid, opt => opt.MapFrom(src => src.Object.Organization.Uuid))
               .ForMember(dto => dto.OrganizationObjectOwnerId, opt => opt.MapFrom(src => src.Object.Organization.ObjectOwnerId))
               .ForMember(dto => dto.OrganizationLastChanged, opt => opt.MapFrom(src => src.Object.Organization.LastChanged))
               .ForMember(dto => dto.OrganizationLastChangedByUserId, opt => opt.MapFrom(src => src.Object.Organization.LastChangedByUserId))
               .ForMember(dto => dto.OrganizationTypeId, opt => opt.MapFrom(src => src.Object.Organization.TypeId))
               .ForMember(dto => dto.ItSystemId, opt => opt.MapFrom(src => src.Object.ItSystem.Id))
               .ForMember(dto => dto.ItSystemItSystemId, opt => opt.MapFrom(src => src.Object.ItSystem.ItSystemId))
               .ForMember(dto => dto.ItSystemParentId, opt => opt.MapFrom(src => src.Object.ItSystem.ParentId))
               .ForMember(dto => dto.ItSystemBusinessTypeId, opt => opt.MapFrom(src => src.Object.ItSystem.BusinessTypeId))
               .ForMember(dto => dto.ItSystemName, opt => opt.MapFrom(src => src.Object.ItSystem.Name))
               .ForMember(dto => dto.ItSystemUuid, opt => opt.MapFrom(src => src.Object.ItSystem.Uuid))
               .ForMember(dto => dto.ItSystemDescription, opt => opt.MapFrom(src => src.Object.ItSystem.Description))
               .ForMember(dto => dto.ItSystemAccessModifier, opt => opt.MapFrom(src => src.Object.ItSystem.AccessModifier))
               .ForMember(dto => dto.ItSystemOrganizationId, opt => opt.MapFrom(src => src.Object.ItSystem.OrganizationId))
               .ForMember(dto => dto.ItSystemBelongsToId, opt => opt.MapFrom(src => src.Object.ItSystem.BelongsToId))
               .ForMember(dto => dto.ItSystemObjectOwnerId, opt => opt.MapFrom(src => src.Object.ItSystem.ObjectOwnerId))
               .ForMember(dto => dto.ItSystemLastChanged, opt => opt.MapFrom(src => src.Object.ItSystem.LastChanged))
               .ForMember(dto => dto.ItSystemLastChangedByUserId, opt => opt.MapFrom(src => src.Object.ItSystem.LastChangedByUserId))
               // .ForMember(dto => dto.ItSystemTerminationDeadlineTypesInSystem_Id, opt => opt.MapFrom(src => src.Object.ItSystem.))
               .ForMember(dto => dto.ItSystemDisabled, opt => opt.MapFrom(src => src.Object.ItSystem.Disabled))
               .ForMember(dto => dto.ItSystemReferenceId, opt => opt.MapFrom(src => src.Object.ItSystem.ReferenceId))
               .ForMember(dto => dto.ItSystemPreviousName, opt => opt.MapFrom(src => src.Object.ItSystem.PreviousName));

            CreateMap<ItProjectRight, RightOutputDTO>();
            CreateMap<RightInputDTO, ItProjectRight>();

            CreateMap<ItContractRight, RightOutputDTO>();
            CreateMap<RightInputDTO, ItContractRight>();

            CreateMap<ItInterfaceExhibit, ItInterfaceExhibitDTO>()
                .ReverseMap();

            CreateMap<DataRow, DataRowDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.DataType, opt => opt.Ignore());

            CreateMap<ItSystem, ItSystemDTO>()
                .ForMember(dest => dest.TaskRefIds, opt => opt.MapFrom(src => src.TaskRefs.Select(x => x.Id)))
                .ForMember(dest => dest.IsUsed, opt => opt.MapFrom(src => src.Usages.Any()))
                .ReverseMap()
                .ForMember(dest => dest.TaskRefs, opt => opt.Ignore())
                .ForMember(dest => dest.ItInterfaceExhibits, opt => opt.Ignore());

            //Simplere mapping than the one above, only one way
            CreateMap<ItSystem, ItSystemSimpleDTO>();

            CreateMap<ItInterface, ItInterfaceDTO>()
                .ForMember(dest => dest.IsUsed, opt => opt.MapFrom(src => src.ExhibitedBy.ItSystem.Usages.Any()))
                .ForMember(dest => dest.BelongsToId, opt => opt.MapFrom(src => src.ExhibitedBy.ItSystem.BelongsTo.Id))
                .ForMember(dest => dest.BelongsToName, opt => opt.MapFrom(src => src.ExhibitedBy.ItSystem.BelongsTo.Name))
                .ReverseMap();

            CreateMap<ItSystemUsage, ItSystemUsageDTO>()
                .ForMember(dest => dest.ResponsibleOrgUnitName,
                    opt => opt.MapFrom(src => src.ResponsibleUsage.OrganizationUnit.Name))
                .ForMember(dest => dest.Contracts, opt => opt.MapFrom(src => src.Contracts.Select(x => x.ItContract)))
                .ForMember(dest => dest.MainContractId, opt => opt.MapFrom(src => src.MainContract.ItContractId))
                .ForMember(dest => dest.MainContractIsActive, opt => opt.MapFrom(src => src.MainContract.ItContract.IsActive))
                .ForMember(dest => dest.InterfaceExhibitCount, opt => opt.MapFrom(src => src.ItSystem.ItInterfaceExhibits.Count))
                .ReverseMap()
                .ForMember(dest => dest.OrgUnits, opt => opt.Ignore())
                .ForMember(dest => dest.TaskRefs, opt => opt.Ignore())
                .ForMember(dest => dest.ItProjects, opt => opt.Ignore())
                .ForMember(dest => dest.Contracts, opt => opt.Ignore());

            //Simplere mapping than the one above, only one way
            CreateMap<ItSystemUsage, ItSystemUsageSimpleDTO>();

            CreateMap<EconomyYear, EconomyYearDTO>()
                  .ReverseMap();

            CreateMap<Risk, RiskDTO>()
                .ReverseMap()
                .ForMember(dest => dest.ItProject, opt => opt.Ignore())
                .ForMember(dest => dest.ResponsibleUser, opt => opt.Ignore());

            CreateMap<Stakeholder, StakeholderDTO>()
                .ReverseMap()
                .ForMember(dest => dest.ItProject, opt => opt.Ignore());

            CreateMap<ItProjectStatus, ItProjectStatusDTO>()
                  .Include<Assignment, AssignmentDTO>()
                  .Include<Milestone, MilestoneDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.AssociatedUser, opt => opt.Ignore())
                  .ForMember(dest => dest.ObjectOwner, opt => opt.Ignore())
                  .ForMember(dest => dest.ObjectOwnerId, opt => opt.Ignore());

            CreateMap<Assignment, AssignmentDTO>()
                  .ReverseMap();
            CreateMap<Milestone, MilestoneDTO>()
                  .ReverseMap();

            CreateMap<GoalStatus, GoalStatusDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.ItProject, opt => opt.Ignore())
                  .ForMember(dest => dest.Goals, opt => opt.Ignore());

            CreateMap<Goal, GoalDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.GoalStatus, opt => opt.Ignore())
                  .ForMember(dest => dest.GoalType, opt => opt.Ignore());

            CreateMap<Stakeholder, StakeholderDTO>()
                  .ReverseMap();

            CreateMap<ItProject, ItProjectDTO>()
                .ForMember(dest => dest.ChildrenIds,
                    opt => opt.MapFrom(x => x.Children.Select(y => y.Id)))
                .ReverseMap()
                .ForMember(dest => dest.Children, opt => opt.Ignore())
                .ForMember(dest => dest.ItSystemUsages, opt => opt.Ignore())
                .ForMember(dest => dest.TaskRefs, opt => opt.Ignore())
                .ForMember(dest => dest.UsedByOrgUnits, opt => opt.Ignore())
                .ForMember(dest => dest.Stakeholders, opt => opt.Ignore());

            CreateMap<ItProjectPhase, ItProjectPhaseDTO>()
                  .ReverseMap();

            //Output only - this mapping should not be reversed
            CreateMap<ItProject, ItProjectCatalogDTO>();

            //Output only - this mapping should not be reversed
            CreateMap<ItProject, ItProjectSimpleDTO>();

            //Output only - this mapping should not be reversed
            CreateMap<ItProject, ItProjectOverviewDTO>()
                .ForMember(dest => dest.ResponsibleOrgUnitName, opt => opt.MapFrom(src => src.ResponsibleUsage.OrganizationUnit.Name));

            CreateMap<Handover, HandoverDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.ItProject, opt => opt.Ignore());

            CreateMap<Communication, CommunicationDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.ItProject, opt => opt.Ignore());
            //AssociatedAgreementElementTypes
            CreateMap<ItContract, ItContractDTO>()
                  .ForMember(dest => dest.AssociatedSystemUsages, opt => opt.MapFrom(src => src.AssociatedSystemUsages.Select(x => x.ItSystemUsage)))
                  .ForMember(dest => dest.AgreementElements, opt => opt.MapFrom(src => src.AssociatedAgreementElementTypes.Select(x => x.AgreementElementType)))
                  .ReverseMap()
                  .ForMember(contract => contract.AssociatedSystemUsages, opt => opt.Ignore())
                  .ForMember(contract => contract.AssociatedAgreementElementTypes, opt => opt.Ignore());

            //Output only - this mapping should not be reversed
            CreateMap<ItContract, ItContractOverviewDTO>();

            //Output only - this mapping should not be reversed
            CreateMap<ItContract, ItContractPlanDTO>();


            //Output only - this mapping should not be reversed
            CreateMap<ItContract, ItContractSystemDTO>()
                .ForMember(dest => dest.AgreementElements, opt => opt.MapFrom(src => src.AssociatedAgreementElementTypes.Select(x => x.AgreementElementType)));

            CreateMap<PaymentMilestone, PaymentMilestoneDTO>()
                  .ReverseMap();

            CreateMap<EconomyStream, EconomyStreamDTO>()
                .ReverseMap();

            CreateMap<Advice, AdviceDTO>()
                  .ReverseMap();

            CreateMap<AdviceUserRelation, AdviceUserRelationDTO>()
                  .ReverseMap();

            CreateMap<HandoverTrial, HandoverTrialDTO>()
                  .ReverseMap();

            //Output only - this mapping should not be reversed
            CreateMap<ExcelImportError, ExcelImportErrorDTO>();

            CreateMap<ItSystemUsageSensitiveDataLevel, ItSystemUsageSensitiveDataLevelDTO>()
                .ForMember(dest => dest.DataSensitivityLevel, opt => opt.MapFrom(src => src.SensitivityDataLevel));
        }
    }
}
