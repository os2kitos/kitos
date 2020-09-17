using System.Collections.Generic;

namespace Core.DomainModel.GDPR.Read
{
    /// <summary>
    /// A READ optimized perspective of the data processing agreement model
    /// </summary>
    public class DataProcessingAgreementReadModel : IOwnedByOrganization, IReadModel<DataProcessingAgreement>
    {
        public DataProcessingAgreementReadModel()
        {
            RoleAssignments = new List<DataProcessingAgreementRoleAssignmentReadModel>();
        }

        public int Id { get; set; }
        
        public string Name { get; set; }

        public int OrganizationId { get; set; }

        public virtual Organization.Organization Organization { get; set; }

        public int SourceEntityId { get; set; }

        public string MainReferenceUserAssignedId { get; set; }

        public string MainReferenceUrl { get; set; }

        public string MainReferenceTitle { get; set; }

        public string SystemNamesAsCsv { get; set; }
        
        public virtual DataProcessingAgreement SourceEntity { get; set; }
        
        public virtual ICollection<DataProcessingAgreementRoleAssignmentReadModel> RoleAssignments { get; set; }
    }
}
