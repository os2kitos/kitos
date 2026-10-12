using Core.DomainModel.Shared;
using Presentation.Web.Models.API.V1.References;
using Presentation.Web.Models.API.V1.Shared;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Presentation.Web.Models.API.V1.GDPR
{
    public class DataProcessingRegistrationDTO : NamedEntityDTO
    {
        [SetsRequiredMembers]
        public DataProcessingRegistrationDTO(int id, string name)
            : base(id, name)
        {
        }

        public Guid Uuid { get; set; }

        public required AssignedRoleDTO[]? AssignedRoles { get; set; }

        public required ReferenceDTO[]? References { get; set; }

        public required NamedEntityWithEnabledStatusDTO[]? ItSystems { get; set; }

        public required ValueWithOptionalRemarkDTO<YearMonthIntervalOption?>? OversightInterval { get; set; }

        public YesNoUndecidedOption? HasSubDataProcessors { get; set; }

        public required ShallowOrganizationDTO[]? DataProcessors { get; set; }
        
        public required SubDataProcessorResponseDTO[]? SubDataProcessors { get; set; }

        public required ValueWithOptionalDateAndRemark<YesNoIrrelevantOption?>? AgreementConcluded { get; set; }

        public YesNoUndecidedOption? TransferToInsecureThirdCountries { get; set; }

        public required NamedEntityWithExpirationStatusDTO[]? InsecureThirdCountries { get; set; }

        public NamedEntityWithExpirationStatusDTO? BasisForTransfer { get; set; }

        public required ValueWithOptionalRemarkDTO<OptionWithDescriptionAndExpirationDTO>? DataResponsible { get; set; }

        public required ValueWithOptionalRemarkDTO<NamedEntityWithExpirationStatusDTO[]>? OversightOptions { get; set; }


        public required ValueWithOptionalRemarkDTO<YesNoUndecidedOption?>? OversightCompleted { get; set; }

        public required DataProcessingRegistrationOversightDateDTO[]? OversightDates { get; set; }

        public required NamedEntityDTO[]? AssociatedContracts { get; set; }
        public DateTime? OversightScheduledInspectionDate { get; set; }
        public string? LastChangedByName { get; set; }
        public DateTime LastChangedAt { get; set; }
        public bool IsActiveAccordingToMainContract { get; set; }
        public int? MainContractId { get; set; }
    }
}