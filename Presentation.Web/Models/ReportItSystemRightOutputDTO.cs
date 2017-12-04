using Core.DomainModel;
using Core.DomainModel.ItSystem;
using System;

namespace Presentation.Web.Models
{
    public class ReportItSystemRightOutputDTO
    {
        public int roleId { get; set; }
        public string roleName { get; set; }
        public bool roleisSuggestion { get; set; }
        public bool roleHasreadAccess { get; set; }
        public bool roleHasWriteAccess { get; set; }
        public int roleObjectOwnerId { get; set; }
        public DateTime roleLastChanged { get; set; }
        public int roleLastChangedByUserId { get; set; }
        public string roleDescription { get; set; }
        public bool roleIsObligatory { get; set; }
        public bool roleIsEnabled { get; set; }
        public bool roleIsLocallyAvailable { get; set; }
        public int rolePriority { get; set; }

        public int itSystemRightId { get; set; }
        public int itSystemRightUserId { get; set; }
        public int itSystemRightRoleId { get; set; }
        public int itSystemRightObjectId { get; set; }
        public int itSystemRightObjectOwnerId { get; set; }
        public DateTime itSystemRightLastChanged { get; set; }
        public int itSystemRightLastChangedByUserId { get; set; }

        public int userId { get; set; }
        public string userName { get; set; }
        public string userLastName { get; set; }
        public string userPhoneNumber { get; set; }
        public string userEmail { get; set; }
        public string userPassword { get; set; }
        public string userSalt { get; set; }
        public bool userIsGlobalAdmin { get; set; }
        public DateTime userLastAdvisDate { get; set; }
        public int userObjectOwnerId { get; set; }
        public DateTime userLastChanged { get; set; }
        public int userLastChangedByUserId { get; set; }
        public int userDefaultOrganizationId { get; set; }
        public DateTime userLockedOutDate { get; set; }
        public int userFailedAttempts { get; set; }
        public string userDefaultUserStartPreference { get; set; }
        public string userUniqueId { get; set; }

        public int ItSystemUsageId { get; set; }
        public bool ItSystemUsageIsStatusActive { get; set; }
        public string ItSystemUsageNote { get; set; }
        public string ItSystemUsageLocalSystemId { get; set; }
        public string ItSystemUsageVersion { get; set; }
        public string ItSystemUsageEsdhRef { get; set; }
        public string ItSystemUsageCmdbRef { get; set; }
        public string ItSystemUsageDirectoryOrUrlRef { get; set; }
        public string ItSystemUsageLocalCallName { get; set; }
        public int ItSystemUsageOrganizationId { get; set; }
        public int ItSystemUsageItSystemId { get; set; }
        public int ItSystemUsageArchiveTypeId { get; set; }
        public int ItSystemUsageSensitiveDataTypeId { get; set; }
        public int ItSystemUsageOverviewId { get; set; }
        public int ItSystemUsageObjectOwnerId { get; set; }
        public DateTime ItSystemUsageLastChanged { get; set; }
        public int ItSystemUsageLastChangedByUserId { get; set; }
        public int ItSystemUsageOrganizationUnit_Id { get; set; }
        public int ItSystemUsageReferenceId { get; set; }
        public bool ItSystemUsageActive { get; set; }
        public DateTime ItSystemUsageConcluded { get; set; }
        public DateTime ItSystemUsageExpirationDate { get; set; }
        public DateTime ItSystemUsageTerminated { get; set; }
        public int ItSystemUsageTerminationDeadlineInSystem_Id { get; set; }
        public bool ItSystemUsageArchiveDuty { get; set; }
        public bool ItSystemUsageArchived { get; set; }
        public bool ItSystemUsageReportedToDPA { get; set; }
        public string ItSystemUsageDocketNo { get; set; }
        public DateTime ItSystemUsageArchivedDate { get; set; }
        public string ItSystemUsageArchiveNotes { get; set; }
        public int ItSystemUsageArchiveLocationId { get; set; }

        public int OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public string OrganizationCvr { get; set; }
        public int OrganizationAccessModifier { get; set; }
        public string OrganizationUuid { get; set; }
        public int OrganizationObjectOwnerId { get; set; }
        public DateTime OrganizationLastChanged { get; set; }
        public int OrganizationLastChangedByUserId { get; set; }
        public int OrganizationTypeId { get; set; }

        public int ItSystemId { get; set; }
        public int ItSystemItSystemId { get; set; }
        public int ItSystemAppTypeOptionId { get; set; }
        public int ItSystemParentId { get; set; }
        public int ItSystemBusinessTypeId { get; set; }
        public string ItSystemName { get; set; }
        public string ItSystemUuid { get; set; }
        public string ItSystemDescription { get; set; }
        public string ItSystemUrl { get; set; }
        public int ItSystemAccessModifier { get; set; }
        public int ItSystemOrganizationId { get; set; }
        public int ItSystemBelongsToId { get; set; }
        public int ItSystemObjectOwnerId { get; set; }
        public DateTime ItSystemLastChanged { get; set; }
        public int ItSystemLastChangedByUserId { get; set; }
        public int ItSystemTerminationDeadlineTypesInSystem_Id { get; set; }
        public bool ItSystemDisabled { get; set; }
        public int ItSystemReferenceId { get; set; }
        public string ItSystemPreviousName { get; set; }
    }
}
