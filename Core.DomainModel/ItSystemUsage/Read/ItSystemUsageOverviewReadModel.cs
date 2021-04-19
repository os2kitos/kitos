using System;
using System.Collections.Generic;

namespace Core.DomainModel.ItSystemUsage.Read
{
    public class ItSystemUsageOverviewReadModel : IOwnedByOrganization, IReadModel<ItSystemUsage>, IHasName
    {

        public static int MaxReferenceTitleLenght = 100;

        public ItSystemUsageOverviewReadModel()
        {
            RoleAssignments = new List<ItSystemUsageOverviewRoleAssignmentReadModel>();
            ItSystemTaskRefs = new List<ItSystemUsageOverviewTaskRefReadModel>();
        }

        public int OrganizationId { get; set; }
        public Organization.Organization Organization { get; set; }
        public int Id { get; set; }
        public int SourceEntityId { get; set; }
        public ItSystemUsage SourceEntity { get; set; }
        public string Name { get; set; }
        public bool ItSystemDisabled { get; set; }
        public bool IsActive { get; set; }
        public string ParentItSystemName { get; set; }
        public int? ParentItSystemId { get; set; }
        public string Version { get; set; }
        public string LocalCallName { get; set; }
        public string LocalSystemId { get; set; }
        public virtual ICollection<ItSystemUsageOverviewRoleAssignmentReadModel> RoleAssignments { get; set; }
        public string ItSystemUuid { get; set; }
        public int? ResponsibleOrganizationUnitId { get; set; }
        public string ResponsibleOrganizationUnitName { get; set; }
        public int? ItSystemBusinessTypeId { get; set; }
        public string ItSystemBusinessTypeName { get; set; }
        public int? ItSystemRightsHolderId { get; set; }
        public string ItSystemRightsHolderName { get; set; }
        public string ItSystemKLEIdsAsCsv { get; set; }
        public string ItSystemKLENamesAsCsv { get; set; }
        public virtual ICollection<ItSystemUsageOverviewTaskRefReadModel> ItSystemTaskRefs { get; set; } // Adding TaskRefs as collection to enable indexed search
        public string LocalReferenceDocumentId { get; set; }
        public string LocalReferenceUrl { get; set; }
        public string LocalReferenceTitle { get; set; }

        public int? ObjectOwnerId { get; set; }
        public string ObjectOwnerName { get; set; }
        public int? LastChangedById { get; set; }
        public string LastChangedByName { get; set; }
        public DateTime LastChanged { get; set; } 
        public DateTime? Concluded { get; set; }
    }
}
