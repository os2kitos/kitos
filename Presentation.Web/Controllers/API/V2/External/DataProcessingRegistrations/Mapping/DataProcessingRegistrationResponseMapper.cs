using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.GDPR;
using Core.DomainModel.Shared;
using Presentation.Web.Controllers.API.V2.Mapping;
using Presentation.Web.Models.API.V2.Response.DataProcessing;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.Response.Generic.Roles;
using Presentation.Web.Models.API.V2.Types.DataProcessing;
using Presentation.Web.Models.API.V2.Types.Shared;

namespace Presentation.Web.Controllers.API.V2.External.DataProcessingRegistrations.Mapping
{
    public class DataProcessingRegistrationResponseMapper : IDataProcessingRegistrationResponseMapper
    {
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
                ExternalReferences = MapExternalReferences(dataProcessingRegistration)
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
                OversightDates = MapOversightDates(dataProcessingRegistration.OversightDates).ToList()
            };
        }

        private OversightIntervalChoice? MapOversightInterval(YearMonthIntervalOption? oversightInterval)
        {
            return oversightInterval?.ToYesNoDontKnowChoice();
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
                CompletedAt = oversightDate.OversightDate,
                Remark = oversightDate.OversightRemark
            };
        }

        private static IEnumerable<IdentityNamePairResponseDTO> MapSystemUsages(DataProcessingRegistration dataProcessingRegistration)
        {
            return dataProcessingRegistration.SystemUsages.Select(x => x.MapIdentityNamePairDTO());
        }

        private DataProcessingRegistrationGeneralDataResponseDTO MapGeneral(DataProcessingRegistration dataProcessingRegistration)
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
                SubDataProcessors = dataProcessingRegistration.SubDataProcessors?.Select(x => x.MapShallowOrganizationResponseDTO()).ToList()
            };
        }

        private static YesNoIrrelevantChoice? MapYesNoIrrelevant(YesNoIrrelevantOption? isAgreementConcluded)
        {
            return isAgreementConcluded?.ToYesNoIrrelevantChoice();
        }

        private static IEnumerable<ExternalReferenceDataDTO> MapExternalReferences(DataProcessingRegistration dataProcessingRegistration)
        {
            return dataProcessingRegistration.ExternalReferences.Select(reference => MapExternalReferenceDTO(dataProcessingRegistration, reference)).ToList();
        }

        private static ExternalReferenceDataDTO MapExternalReferenceDTO(DataProcessingRegistration dataProcessingRegistration, ExternalReference reference)
        {
            return new()
            {
                DocumentId = reference.ExternalReferenceId,
                Title = reference.Title,
                Url = reference.URL,
                MasterReference = dataProcessingRegistration.Reference?.Id.Equals(reference.Id) == true
            };
        }

        private static IEnumerable<RoleAssignmentResponseDTO> MapRoles(DataProcessingRegistration dataProcessingRegistration)
        {
            return dataProcessingRegistration.Rights.Select(ToRoleResponseDTO).ToList();
        }

        private static RoleAssignmentResponseDTO ToRoleResponseDTO(DataProcessingRegistrationRight right)
        {
            return new()
            {
                Role = right.Role.MapIdentityNamePairDTO(),
                User = right.User.MapIdentityNamePairDTO()
            };
        }
    }
}