﻿using System;
using System.Collections.Generic;
using Core.DomainModel.ItSystem.DataTypes;

namespace Core.DomainModel.ItSystemUsage.Read
{
    public class ItSystemUsageOverviewReadModel : IOwnedByOrganization, IReadModel<ItSystemUsage>, IHasName
    {

        public static int MaxReferenceTitleLenght = 100;

        public ItSystemUsageOverviewReadModel()
        {
            RoleAssignments = new List<ItSystemUsageOverviewRoleAssignmentReadModel>();
            ItSystemTaskRefs = new List<ItSystemUsageOverviewTaskRefReadModel>();
            SensitiveDataLevels = new List<ItSystemUsageOverviewSensitiveDataLevelReadModel>();
            ItProjects = new List<ItSystemUsageOverviewItProjectReadModel>();
            ArchivePeriods = new List<ItSystemUsageOverviewArchivePeriodReadModel>();
            DataProcessingRegistrations = new List<ItSystemUsageOverviewDataProcessingRegistrationReadModel>();
            DependsOnInterfaces = new List<ItSystemUsageOverviewInterfaceReadModel>();
            IncomingRelatedItSystemUsages = new List<ItSystemUsageOverviewItSystemUsageReadModel>();
        }


        public DateTime? ActiveArchivePeriodEndDate
        {
            get
            {
                ItSystemUsageOverviewArchivePeriodReadModel archivePeriodWithEarliestStartDate = null;
                var today = DateTime.Now;
                foreach (var archivePeriod in ArchivePeriods)
                {
                    if (today >= archivePeriod.StartDate && today <= archivePeriod.EndDate)
                    {
                        if (archivePeriodWithEarliestStartDate == null)
                        {
                            archivePeriodWithEarliestStartDate = archivePeriod;
                        }
                        else if (archivePeriodWithEarliestStartDate?.StartDate > archivePeriod.StartDate)
                        {
                            archivePeriodWithEarliestStartDate = archivePeriod;
                        }
                    }
                }
                return archivePeriodWithEarliestStartDate?.EndDate;
            }
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
        public bool? ParentItSystemDisabled { get; set; }
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

        public int? MainContractId { get; set; }
        public int? MainContractSupplierId { get; set; }
        public string MainContractSupplierName { get; set; }
        public bool? MainContractIsActive { get; set; }
        public bool HasMainContract { get; set; }

        public string SensitiveDataLevelsAsCsv { get; set; }
        public virtual ICollection<ItSystemUsageOverviewSensitiveDataLevelReadModel> SensitiveDataLevels { get; set; }

        public string ItProjectNamesAsCsv { get; set; }
        public virtual ICollection<ItSystemUsageOverviewItProjectReadModel> ItProjects { get; set; } // Adding ItProjects as collection to enable indexed search

        public ArchiveDutyTypes? ArchiveDuty { get; set; }
        public bool IsHoldingDocument { get; set; }

        public virtual ICollection<ItSystemUsageOverviewArchivePeriodReadModel> ArchivePeriods { get; set; }

        public string RiskSupervisionDocumentationName { get; set; }
        public string RiskSupervisionDocumentationUrl { get; set; }
        public string LinkToDirectoryName { get; set; }
        public string LinkToDirectoryUrl { get; set; }

        public string DataProcessingRegistrationsConcludedAsCsv { get; set; }
        public string DataProcessingRegistrationNamesAsCsv { get; set; }
        public virtual ICollection<ItSystemUsageOverviewDataProcessingRegistrationReadModel> DataProcessingRegistrations { get; set; }

        public string GeneralPurpose { get; set; }
        public HostedAt HostedAt { get; set; }

        public string DependsOnInterfacesNamesAsCsv { get; set; }
        public virtual ICollection<ItSystemUsageOverviewInterfaceReadModel> DependsOnInterfaces { get; set; }

        public string IncomingRelatedItSystemUsagesNamesAsCsv { get; set; }
        public virtual ICollection<ItSystemUsageOverviewItSystemUsageReadModel> IncomingRelatedItSystemUsages { get; set; }
    }
}