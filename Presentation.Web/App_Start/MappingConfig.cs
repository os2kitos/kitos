using System;
using System.Globalization;
using System.Linq;
using AutoMapper;
using Core.ApplicationServices;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainModel.Advice;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItSystemUsage.GDPR;
using Presentation.Web.Extensions;
using Presentation.Web.Infrastructure;
using Presentation.Web.Models.API.V1;
using Presentation.Web.Models.API.V1.ItSystemUsage;
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
                cfg.AllowNullDestinationValues = true;
                cfg.AddProfile<MappingProfile>();
                cfg.AddProfile<DropdownProfile>();
            });

            //Precompile automapper mappings to take performance hit during application boot in stead of during first request.
            mappingconfig.CompileMappings();

            return mappingconfig.CreateMapper();
        }
    }

    public class DropdownProfile : Profile
    {
        public DropdownProfile()
        {
            CreateMap<ContactPerson, ContactPersonDTO>()
                .ReverseMap()
                .ForMember(x => x.Organization, opt => opt.UseDestinationValue())
                .IgnoreDestinationEntityFields();

            CreateMap<ExternalReference, ExternalReferenceDTO>()
                .ReverseMap()
                .ForMember(x => x.Uuid, opt => opt.Ignore())
                .IgnoreDestinationEntityFields();

            CreateMap<AgreementElementType, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore())
                  .IgnoreDestinationEntityFields();

            CreateMap<ArchiveType, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore())
                  .IgnoreDestinationEntityFields();

            CreateMap<ItContractTemplateType, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore())
                  .IgnoreDestinationEntityFields();

            CreateMap<ItContractType, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore())
                  .IgnoreDestinationEntityFields();

            CreateMap<InterfaceType, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore())
                  .IgnoreDestinationEntityFields();

            CreateMap<DataType, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore())
                  .IgnoreDestinationEntityFields();

            CreateMap<PurchaseFormType, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore())
                  .IgnoreDestinationEntityFields();

            CreateMap<RelationFrequencyType, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore())
                  .IgnoreDestinationEntityFields();

            CreateMap<BusinessType, OptionDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore())
                  .IgnoreDestinationEntityFields();

            CreateMap<SensitiveDataType, OptionDTO>()
                .ReverseMap()
                .ForMember(dest => dest.References, opt => opt.Ignore())
                .IgnoreDestinationEntityFields();

            CreateMap<ProcurementStrategyType, OptionDTO>()
                .ReverseMap()
                .ForMember(dest => dest.References, opt => opt.Ignore())
                .IgnoreDestinationEntityFields();

            CreateMap<OptionExtendType, OptionDTO>()
                .ReverseMap()
                .ForMember(dest => dest.References, opt => opt.Ignore())
                .IgnoreDestinationEntityFields();

            CreateMap<PriceRegulationType, OptionDTO>()
                .ReverseMap()
                .ForMember(dest => dest.References, opt => opt.Ignore())
                .IgnoreDestinationEntityFields();

            CreateMap<PaymentModelType, OptionDTO>()
                .ReverseMap()
                .ForMember(dest => dest.References, opt => opt.Ignore())
                .IgnoreDestinationEntityFields();

            CreateMap<PaymentFreqencyType, OptionDTO>()
                .ReverseMap()
                .ForMember(dest => dest.References, opt => opt.Ignore())
                .IgnoreDestinationEntityFields();

            CreateMap<TerminationDeadlineType, OptionDTO>()
                .ReverseMap()
                .ForMember(dest => dest.References, opt => opt.Ignore())
                .IgnoreDestinationEntityFields();

            CreateMap<CriticalityType, OptionDTO>()
                .ReverseMap()
                .ForMember(dest => dest.References, opt => opt.Ignore())
                .IgnoreDestinationEntityFields();

            CreateMap<ItSystemRole, RoleDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore())
                  .IgnoreDestinationEntityFields();

            CreateMap<ItContractRole, RoleDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore())
                  .IgnoreDestinationEntityFields();

            CreateMap<OrganizationUnitRole, RoleDTO>()
                  .ReverseMap()
                  .ForMember(dest => dest.References, opt => opt.Ignore())
                  .IgnoreDestinationEntityFields();

            CreateMap<Config, ConfigDTO>()
                .ReverseMap()
                .IgnoreDestinationEntityFields();

            CreateMap<DataProtectionAdvisor, DataProtectionAdvisorDTO>()
                  .ReverseMap()
                  .IgnoreDestinationEntityFields();

            CreateMap<DataResponsible, DataResponsibleDTO>()
                  .ReverseMap()
                  .IgnoreDestinationEntityFields();
        }
    }

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Text, TextDTO>()
                  .ReverseMap()
                  .IgnoreDestinationEntityFields();

            CreateMap<User, UserDTO>()
                  .ForMember(dest => dest.DefaultOrganizationUnitId,
                      opt => opt.MapFrom(src => src.OrganizationRights.FirstOrDefault() != null ? src.OrganizationRights.First().DefaultOrgUnitId : null))
                  .ForMember(dest => dest.DefaultOrganizationUnitUuid,
                      opt => opt.MapFrom(src => src.OrganizationRights.FirstOrDefault() != null ? src.OrganizationRights.First().DefaultOrgUnit.Uuid : (Guid?)null))
                  .ForMember(dest => dest.DefaultOrganizationUnitName,
                      opt => opt.MapFrom(src => src.OrganizationRights.FirstOrDefault() != null ? src.OrganizationRights.First().DefaultOrgUnit.Name : null))
                  .ReverseMap()
                  .IgnoreDestinationEntityFields();

            CreateMap<Organization, OrganizationDTO>()
                .ForMember(dest => dest.Root, opt => opt.MapFrom(src => src.GetRoot()))
                .ReverseMap()
                .IgnoreDestinationEntityFields();

            CreateMap<Organization, OrganizationSimpleDTO>();

            CreateMap<OrganizationUnit, OrgUnitDTO>()
                  .ForMember(dest => dest.Children, opt => opt.MapFrom(unit => unit.Children.OrderBy(child => child.Name, KitosConstants.DanishStringComparer).ToList()))
                  .ReverseMap()
                  .ForMember(dest => dest.Children, opt => opt.Ignore())
                  .IgnoreDestinationEntityFields();

            CreateMap<OrganizationUnit, OrgUnitSimpleDTO>();
            CreateMap<OrganizationUnit, SimpleOrgUnitDTO>();

            CreateMap<PasswordResetRequest, PasswordResetRequestDTO>()
                  .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email));

            CreateMap<TaskRef, TaskRefDTO>()
                  .ReverseMap()
                  .IgnoreDestinationEntityFields();

            CreateMap<OrganizationRight, OrganizationRightDTO>()
                .ForMember(dest => dest.OrganizationUuid, opt => opt.MapFrom(src => src.Organization.Uuid))
                .ReverseMap()
                .IgnoreDestinationEntityFields();

            CreateMap<OrganizationUnitRight, RightOutputDTO>();
            CreateMap<RightInputDTO, OrganizationUnitRight>();

            CreateMap<ItSystemRight, RightOutputDTO>()
                .ForMember(dto => dto.ObjectName, opt => opt.MapFrom(src => src.Object.ItSystem.Name));
            CreateMap<RightInputDTO, ItSystemRight>();

            CreateMap<ItContractRight, RightOutputDTO>();
            CreateMap<RightInputDTO, ItContractRight>();

            CreateMap<ItInterfaceExhibit, ItInterfaceExhibitDTO>()
                .ReverseMap()
                .IgnoreDestinationEntityFields();

            CreateMap<DataRow, DataRowDTO>()
                  .ReverseMap()
                  .IgnoreDestinationEntityFields();

            CreateMap<ItSystem, ItSystemDTO>()
                .ForMember(dest => dest.TaskRefIds, opt => opt.MapFrom(src => src.TaskRefs.Select(x => x.Id)))
                .ForMember(dest => dest.IsUsed, opt => opt.MapFrom(src => src.Usages.Any()))
                .ReverseMap()
                .ForMember(dest => dest.TaskRefs, opt => opt.Ignore())
                .ForMember(dest => dest.ItInterfaceExhibits, opt => opt.Ignore())
                .IgnoreDestinationEntityFields();

            //Simplere mapping than the one above, only one way
            CreateMap<ItSystem, ItSystemSimpleDTO>();

            CreateMap<ItInterface, ItInterfaceDTO>()
                .ForMember(dest => dest.IsUsed, opt => opt.MapFrom(src => src.ExhibitedBy.ItSystem.Usages.Any()))
                .ForMember(dest => dest.BelongsToId, opt => opt.MapFrom(src => src.ExhibitedBy.ItSystem.BelongsTo.Id))
                .ForMember(dest => dest.BelongsToName,
                    opt => opt.MapFrom(src => src.ExhibitedBy.ItSystem.BelongsTo.Name))
                .ReverseMap()
                .IgnoreDestinationEntityFields();

            CreateMap<ItSystemUsage, ItSystemUsageDTO>()
                .ForMember(dest => dest.ResponsibleOrgUnitName,
                    opt => opt.MapFrom(src => src.ResponsibleUsage.OrganizationUnit.Name))
                .ForMember(dest => dest.Contracts, opt => opt.MapFrom(src => src.Contracts.Select(x => x.ItContract)))
                .ForMember(dest => dest.MainContractId, opt => opt.MapFrom(src => src.MainContract.ItContractId))
                .ForMember(dest => dest.MainContractIsActive, opt => opt.MapFrom(src => src.MainContract.ItContract.IsActive))
                .ForMember(dest => dest.InterfaceExhibitCount, opt => opt.MapFrom(src => src.ItSystem.ItInterfaceExhibits.Count))
                .ForMember(dest => dest.PersonalData, opt => opt.MapFrom(src => src.PersonalDataOptions.Select(x => x.PersonalData)))
                .ReverseMap()
                .ForMember(dest => dest.TaskRefs, opt => opt.Ignore())
                .ForMember(dest => dest.Contracts, opt => opt.Ignore())
                .IgnoreDestinationEntityFields();

            //Simplere mapping than the one above, only one way
            CreateMap<ItSystemUsage, ItSystemUsageSimpleDTO>();

            //AssociatedAgreementElementTypes
            CreateMap<ItContract, ItContractDTO>()
                  .ForMember(dest => dest.AssociatedSystemUsages, opt => opt.MapFrom(src => src.AssociatedSystemUsages.Select(x => x.ItSystemUsage)))
                  .ForMember(dest => dest.AgreementElements, opt => opt.MapFrom(src => src.AssociatedAgreementElementTypes.Select(x => x.AgreementElementType)))
                  .ForMember(dest => dest.LastChangedByName, opt => opt.MapFrom(src => src.LastChangedByUser.GetFullName()))
                  .ForMember(dest => dest.ObjectOwnerFullName, opt => opt.MapFrom(src => src.ObjectOwner.GetFullName()))
                  .ReverseMap()
                  .ForMember(contract => contract.AssociatedSystemUsages, opt => opt.Ignore())
                  .ForMember(contract => contract.AssociatedAgreementElementTypes, opt => opt.Ignore())
                  .IgnoreDestinationEntityFields();

            //Output only - this mapping should not be reversed
            CreateMap<ItContract, ItContractSystemDTO>()
                .ForMember(dest => dest.AgreementElements, opt => opt.MapFrom(src => src.AssociatedAgreementElementTypes.Select(x => x.AgreementElementType)));

            CreateMap<EconomyStream, EconomyStreamDTO>()
                .ReverseMap()
                .IgnoreDestinationEntityFields();

            CreateMap<Advice, AdviceDTO>()
                  .ReverseMap()
                  .IgnoreDestinationEntityFields();

            CreateMap<AdviceUserRelation, AdviceUserRelationDTO>()
                  .ReverseMap()
                  .IgnoreDestinationEntityFields();

            //Output only - this mapping should not be reversed
            CreateMap<ExcelImportError, ExcelImportErrorDTO>();

            CreateMap<ItSystemUsageSensitiveDataLevel, ItSystemUsageSensitiveDataLevelDTO>()
                .ForMember(dest => dest.DataSensitivityLevel, opt => opt.MapFrom(src => src.SensitivityDataLevel));

            //DPR
            CreateMap<DataProcessingRegistration, NamedEntityDTO>();
        }
    }
}
