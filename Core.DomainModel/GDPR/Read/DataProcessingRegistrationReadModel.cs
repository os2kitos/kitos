using System;
using System.Collections.Generic;
using Core.DomainModel.Shared;

namespace Core.DomainModel.GDPR.Read
{
    /// <summary>
    /// A READ optimized perspective of the data processing registration model
    /// </summary>
    public class DataProcessingRegistrationReadModel : IOwnedByOrganization, IReadModel<DataProcessingRegistration>, IHasName
    {
        public DataProcessingRegistrationReadModel()
        {
            RoleAssignments = new List<DataProcessingRegistrationRoleAssignmentReadModel>();
        }

        public int Id { get; set; }
        
        public string Name { get; set; }

        public int OrganizationId { get; set; }

        public virtual Organization.Organization Organization { get; set; }

        public int SourceEntityId { get; set; }
        public Guid SourceEntityUuid { get; set; }

        public string MainReferenceUserAssignedId { get; set; }

        public string MainReferenceUrl { get; set; }

        public string MainReferenceTitle { get; set; }

        public string SystemNamesAsCsv { get; set; }
        public string SystemUuidsAsCsv { get; set; }
        
        public string DataProcessorNamesAsCsv { get; set; }
        
        public string SubDataProcessorNamesAsCsv { get; set; }

        public virtual DataProcessingRegistration SourceEntity { get; set; }

        public virtual ICollection<DataProcessingRegistrationRoleAssignmentReadModel> RoleAssignments { get; set; }

        public YesNoIrrelevantOption? IsAgreementConcluded { get; set; }
        
        public YesNoUndecidedOption? TransferToInsecureThirdCountries { get; set; }

        public DateTime? AgreementConcludedAt { get; set; }

        public DateTime? LatestOversightDate { get; set; }
        public string LatestOversightRemark { get; set; }

        public string BasisForTransfer{ get; set; }

        public Guid? BasisForTransferUuid { get; set; }

        public YearMonthIntervalOption? OversightInterval { get; set; }

        public string DataResponsible{ get; set; }
        public Guid? DataResponsibleUuid { get; set; }

        public string OversightOptionNamesAsCsv { get; set; }

        public YesNoUndecidedOption? IsOversightCompleted { get; set; }

        public DateTime? OversightScheduledInspectionDate { get; set; }
        public bool IsActive { get; set; }
        public bool ActiveAccordingToMainContract { get; set; }

        public string ContractNamesAsCsv { get; set; }
        public int? LastChangedById { get; set; }
        public string LastChangedByName { get; set; }
        public DateTime LastChangedAt { get; set; }
    }
}
