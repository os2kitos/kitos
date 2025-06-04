using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Model.GDPR;
using Core.DomainModel.GDPR;
using Core.DomainModel.Shared;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Controllers.API.V2.External.Generic;
using Presentation.Web.Models.API.V2.Response.DataProcessing;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.Response.Generic.Roles;
using Presentation.Web.Models.API.V2.Types.DataProcessing;
using Presentation.Web.Models.API.V2.Types.Shared;

namespace Presentation.Web.Controllers.API.V2.External.DataProcessingRegistrations.Mapping
{
    public class DataProcessingRegistrationResponseMapper : IDataProcessingRegistrationResponseMapper
    {
        private readonly IExternalReferenceResponseMapper _externalReferenceResponseMapper;

        public DataProcessingRegistrationResponseMapper(IExternalReferenceResponseMapper externalReferenceResponseMapper)
        {
            _externalReferenceResponseMapper = externalReferenceResponseMapper;
        }

        public DataProcessingRegistrationResponseDTO MapDataProcessingRegistrationDTO(DataProcessingRegistration dataProcessingRegistration)
        {
            return new DataProcessingRegistrationResponseDTO
            {
                Uuid = dataProcessingRegistration.Uuid,
                Name = dataProcessingRegistration.Name,
                OrganizationContext = dataProcessingRegistration.Organization?.MapShallowOrganizationResponseDTO(),
                CreatedBy = dataProcessingRegistration.ObjectOwner?.MapIdentityNamePairDTO(),
                LastModified = dataProcessingRegistration.LastChanged,
                LastModifiedBy = dataProcessingRegistration.LastChangedByUser?.MapIdentityNamePairDTO(),
                General = MapGeneral(dataProcessingRegistration),
                SystemUsages = MapSystemUsages(dataProcessingRegistration),
                Oversight = MapOversight(dataProcessingRegistration),
                Roles = MapRoles(dataProcessingRegistration),
                ExternalReferences = _externalReferenceResponseMapper.MapExternalReferences(dataProcessingRegistration.ExternalReferences)
            };
        }

        public DataProcessingRegistrationPermissionsResponseDTO MapPermissions(DataProcessingRegistrationPermissions permissions)
        {
            return new DataProcessingRegistrationPermissionsResponseDTO
            {
                Delete = permissions.BasePermissions.Delete,
                Modify = permissions.BasePermissions.Modify,
                Read = permissions.BasePermissions.Read
            };
        }

        private DataProcessingRegistrationOversightResponseDTO MapOversight(DataProcessingRegistration dataProcessingRegistration)
        {
            return new DataProcessingRegistrationOversightResponseDTO
            {
                OversightOptions = dataProcessingRegistration.OversightOptions?.Select(x => x.MapIdentityNamePairDTO()).ToList(),
                OversightOptionsRemark = dataProcessingRegistration.OversightOptionRemark,
                OversightInterval = MapOversightInterval(dataProcessingRegistration.OversightInterval),
                OversightIntervalRemark = dataProcessingRegistration.OversightIntervalRemark,
                IsOversightCompleted = MapYesNoUndecided(dataProcessingRegistration.IsOversightCompleted),
                OversightCompletedRemark = dataProcessingRegistration.OversightCompletedRemark,
                OversightScheduledInspectionDate = dataProcessingRegistration.OversightScheduledInspectionDate,
                OversightDates = MapOversightDates(dataProcessingRegistration.OversightDates).ToList()
            };
        }

        private OversightIntervalChoice? MapOversightInterval(YearMonthIntervalOption? oversightInterval)
        {
            return oversightInterval?.ToIntervalChoice();
        }

        private static YesNoUndecidedChoice? MapYesNoUndecided(YesNoUndecidedOption? yesNoUndecidedOption)
        {
            return yesNoUndecidedOption?.ToYesNoUndecidedChoice();
        }

        private IEnumerable<OversightDateDTO> MapOversightDates(ICollection<DataProcessingRegistrationOversightDate> oversightDates)
        {
            return oversightDates.Select(MapOversightDate);
        }

        private static OversightDateDTO MapOversightDate(DataProcessingRegistrationOversightDate oversightDate)
        {
            return new OversightDateDTO
            {
                Uuid = oversightDate.Uuid,
                CompletedAt = oversightDate.OversightDate,
                Remark = oversightDate.OversightRemark
            };
        }

        private static IEnumerable<IdentityNamePairResponseDTO> MapSystemUsages(DataProcessingRegistration dataProcessingRegistration)
        {
            return dataProcessingRegistration.SystemUsages.Select(x => x.MapIdentityNamePairDTO()).ToList();
        }

        private static DataProcessingRegistrationGeneralDataResponseDTO MapGeneral(DataProcessingRegistration dataProcessingRegistration)
        {
            return new DataProcessingRegistrationGeneralDataResponseDTO
            {
                DataResponsible = dataProcessingRegistration.DataResponsible?.MapIdentityNamePairDTO(),
                DataResponsibleRemark = dataProcessingRegistration.DataResponsibleRemark,
                IsAgreementConcluded = MapYesNoIrrelevant(dataProcessingRegistration.IsAgreementConcluded),
                IsAgreementConcludedRemark = dataProcessingRegistration.AgreementConcludedRemark,
                AgreementConcludedAt = dataProcessingRegistration.AgreementConcludedAt,
                TransferToInsecureThirdCountries = MapYesNoUndecided(dataProcessingRegistration.TransferToInsecureThirdCountries),
                BasisForTransfer = dataProcessingRegistration.BasisForTransfer?.MapIdentityNamePairDTO(),
                InsecureCountriesSubjectToDataTransfer = dataProcessingRegistration.InsecureCountriesSubjectToDataTransfer?.Select(x => x.MapIdentityNamePairDTO()).ToList(),
                DataProcessors = dataProcessingRegistration.DataProcessors?.Select(x => x.MapShallowOrganizationResponseDTO()).ToList(),
                HasSubDataProcessors = MapYesNoUndecided(dataProcessingRegistration.HasSubDataProcessors),
                SubDataProcessors = dataProcessingRegistration.AssignedSubDataProcessors?.Select(ToSubDataProcessorDTO).ToList(),
                MainContract = dataProcessingRegistration.MainContract?.MapIdentityNamePairDTO(),
                AssociatedContracts = dataProcessingRegistration.AssociatedContracts?.Select(x => x.MapIdentityNamePairDTO()).ToList(),
                Valid = dataProcessingRegistration.IsActiveAccordingToMainContract,
                ResponsibleOrganizationUnit = dataProcessingRegistration.ResponsibleOrganizationUnit?.MapIdentityNamePairDTO()
            };
        }

        private static DataProcessorRegistrationSubDataProcessorResponseDTO ToSubDataProcessorDTO(SubDataProcessor organization)
        {
            return new DataProcessorRegistrationSubDataProcessorResponseDTO()
            {
                DataProcessorOrganization = organization.Organization.MapShallowOrganizationResponseDTO(),
                BasisForTransfer = organization.SubDataProcessorBasisForTransfer?.MapIdentityNamePairDTO(),
                TransferToInsecureThirdCountry = organization.TransferToInsecureCountry?.ToYesNoUndecidedChoice(),
                InsecureThirdCountrySubjectToDataProcessing = organization.InsecureCountry?.MapIdentityNamePairDTO()
            };
        }

        private static YesNoIrrelevantChoice? MapYesNoIrrelevant(YesNoIrrelevantOption? isAgreementConcluded)
        {
            return isAgreementConcluded?.ToYesNoIrrelevantChoice();
        }

        private static IEnumerable<RoleAssignmentResponseDTO> MapRoles(DataProcessingRegistration dataProcessingRegistration)
        {
            return dataProcessingRegistration.Rights.Select(ToRoleResponseDTO).ToList();
        }

        private static RoleAssignmentResponseDTO ToRoleResponseDTO(DataProcessingRegistrationRight right)
        {
            return new RoleAssignmentResponseDTO
            {
                Role = right.Role.MapIdentityNamePairDTO(),
                User = right.User.MapIdentityNamePairDTO()
            };
        }
    }
}