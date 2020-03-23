using System;
using System.Collections.Generic;
using Core.DomainModel;
using Presentation.Web.Models.ItSystemUsage;

namespace Presentation.Web.Models
{
    using Core.DomainModel.ItSystem;
    using Core.DomainModel.ItSystem.DataTypes;

    public class ItSystemUsageDTO
    {
        public ItSystemUsageDTO(){
            this.AssociatedDataWorkers = new List<ItSystemUsageDataWorkerRelationDTO>();
        }
        public int Id { get; set; }
        public bool IsStatusActive { get; set; }
        public string Note { get; set; }
        public string LocalSystemId { get; set; }
        public string Version { get; set; }
        public string EsdhRef { get; set; }
        public string CmdbRef { get; set; }
        public string DirectoryOrUrlRef { get; set; }
        public string LocalCallName { get; set; }

        public int? SensitiveDataTypeId { get; set; }
        public string SensitiveDataTypeName { get; set; }
        public int? ArchiveTypeId { get; set; }
        public string ArchiveTypeName { get; set; }
        public int? ArchiveLocationId { get; set; }
        public string ArchiveLocationName { get; set; }

        public int? ArchiveTestLocationId { get; set; }

        public string ArchiveTestLocationName { get; set; }

        public string ArchiveSupplier { get; set; }
        public int SupplierId { get; set; }

        public string ResponsibleOrgUnitName { get; set; }

        public int OrganizationId { get; set; }
        public OrganizationDTO Organization { get; set; }

        public bool MainContractIsActive { get; set; }

        public int ItSystemId { get; set; }
        public ItSystemDTO ItSystem { get; set; }

        public string ItSystemParentName { get; set; }

        public int? OverviewId { get; set; }
        public string OverviewItSystemName { get; set; }

        public IEnumerable<RightOutputDTO> Rights { get; set; }

        public IEnumerable<TaskRefDTO> TaskRefs { get; set; }

        public int InterfaceExhibitCount { get; set; }
        public int InterfaceUseCount { get; set; }
        public int ActiveInterfaceUseCount { get; set; }

        public IEnumerable<ItInterfaceUsageDTO> InterfaceUsages { get; set; }
        public IEnumerable<ItInterfaceExposureDTO> InterfaceExposures { get; set; }

        public IEnumerable<ItProjectDTO> ItProjects { get; set; }

        public int? MainContractId { get; set; }
        public IEnumerable<ItContractSystemDTO> Contracts { get; set; }

        public string ObjectOwnerName { get; set; }
        public string ObjectOwnerLastName { get; set; }
        public string ObjectOwnerFullName
        {
            get { return ObjectOwnerName + " " + ObjectOwnerLastName; }
        }
        public ICollection<ExternalReferenceDTO> ExternalReferences { get; set; }
        public int? ReferenceId { get; set; }
        public ExternalReferenceDTO Reference;

        public bool? Active { get; set; }

        public bool IsActive { get; set; }

        public DateTime? Concluded { get; set; }

        public DateTime? ExpirationDate { get; set; }

        public int? ItSystemCategoriesId { get; set; }

        public UserCount UserCount { get; set; }

        public ArchiveDutyTypes? ArchiveDuty { get; set; }
        public bool? ReportedToDPA { get; set; }
        public string DocketNo { get; set; }
        public string ArchiveNotes { get; set; }
        public int? ArchiveFreq { get; set; }
        public bool? Registertype { get; set; }
        public bool? ArchiveFromSystem { get; set; }

        #region GDPR
        public string GeneralPurpose { get; set; }
        public DataOptions? IsBusinessCritical { get; set; }

        public string DataProcessor { get; set; }
        public virtual ICollection<ItSystemUsageDataWorkerRelationDTO> AssociatedDataWorkers { get; set; }

        public DataOptions? DataProcessorControl { get; set; }
        public DateTime? LastControl { get; set; }
        public string DatahandlerSupervisionDocumentationUrlName { get; set; }
        public string DatahandlerSupervisionDocumentationUrl { get; set; }

        public string NoteUsage { get; set; }
        public string LinkToDirectoryUrlName { get; set; }
        public string LinkToDirectoryUrl { get; set; }


        public ICollection<ItSystemUsageSensitiveDataLevelDTO> SensitiveDataLevels { get; set; }

        public DataOptions? Precautions { get; set; }
        public bool PrecautionsOptionsEncryption { get; set; }
        public bool PrecautionsOptionsPseudonomisering { get; set; }
        public bool PrecautionsOptionsAccessControl { get; set; }
        public bool PrecautionsOptionsLogning { get; set; }
        public string TechnicalSupervisionDocumentationUrlName { get; set; }
        public string TechnicalSupervisionDocumentationUrl { get; set; }

        public DataOptions? UserSupervision { get; set; }
        public DateTime UserSupervisionDate { get; set; }
        public string UserSupervisionDocumentationUrlName { get; set; }
        public string UserSupervisionDocumentationUrl { get; set; }

        public DataOptions? RiskAssessment { get; set; }
        public DateTime? RiskAssesmentDate { get; set; }
        public RiskLevel? PreRiskAssessment { get; set; }
        public string RiskSupervisionDocumentationUrlName { get; set; }
        public string RiskSupervisionDocumentationUrl { get; set; }
        public string NoteRisks { get; set; }

        public DataOptions? DPIA { get; set; }
        public DateTime? DPIADateFor { get; set; }
        public string DPIASupervisionDocumentationUrlName { get; set; }
        public string DPIASupervisionDocumentationUrl { get; set; }

        public DataOptions? AnsweringDataDPIA { get; set; }
        public DateTime? DPIADeleteDate { get; set; }
        public int NumberDPIA { get; set; }

        #endregion
    }
}
