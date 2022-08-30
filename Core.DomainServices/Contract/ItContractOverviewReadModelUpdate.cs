using Core.DomainModel.ItContract;
using Core.DomainModel.ItContract.Read;
using Core.DomainServices.Model;

namespace Core.DomainServices.Contract
{
    public class ItContractOverviewReadModelUpdate : IReadModelUpdate<ItContract, ItContractOverviewReadModel>
    {
        public void Apply(ItContract source, ItContractOverviewReadModel destination)
        {
            PatchSourceRelationshipInformation(source, destination);

            //Simple properties
            destination.Name = source.Name;
            destination.IsActive = source.IsActive;
            destination.ContractId = source.ItContractId;
            destination.ParentContractName = source.Parent?.Name;
            destination.SupplierName = source.Supplier?.Name;
            destination.ContractSigner = source.ContractSigner;
            destination.ProcurementInitiated = source.ProcurementInitiated;
            destination.OperationRemunerationBegunDate = source.OperationRemunerationBegun;
            destination.IrrevocableTo = source.IrrevocableTo;
            destination.TerminatedAt = source.Terminated;
            destination.LastEditedAtDate = source.LastChanged;
            destination.LastEditedByUserName = source.LastChangedByUser?.GetFullName();

            //Criticality
            destination.CriticalityId = source.Criticality?.Id;
            destination.CriticalityName = source.Criticality?.Name;

            //ResponsibleOrgUnit
            destination.ResponsibleOrgUnitId = source.ResponsibleOrganizationUnit?.Id;
            destination.ResponsibleOrgUnitName = source.ResponsibleOrganizationUnit?.Name;

            //Contract type
            destination.ContractTypeId = source.ContractType?.Id;
            destination.ContractTypeName = source.ContractType?.Name;

            //Contract template
            destination.ContractTemplateId = source.ContractTemplate?.Id;
            destination.ContractTemplateName = source.ContractTemplate?.Name;

            //purchase form
            destination.PurchaseFormId = source.PurchaseForm?.Id;
            destination.PurchaseFormName = source.PurchaseForm?.Name;

            //procurement strategy
            destination.ProcurementStrategyId = source.ProcurementStrategy?.Id;
            destination.ProcurementStrategyName = source.ProcurementStrategy?.Name;

            //procurement plan
            destination.ProcurementPlanYear = source.ProcurementPlanYear;
            destination.ProcurementPlanQuarter = source.ProcurementPlanQuarter;

            MapRoleAssignments(source, destination);

            MapDataProcessingAgreements(source, destination);

            MapSystemUsages(source, destination);

            //Relations
            destination.NumberOfAssociatedSystemRelations = source.AssociatedSystemRelations.Count;

            //Reference
            destination.ActiveReferenceTitle = source.Reference?.Title;
            destination.ActiveReferenceUrl = source.Reference?.URL;
            destination.ActiveReferenceExternalReferenceId = source.Reference?.ExternalReferenceId;

            MapEconomyStreams(source, destination);

            //Payment model
            destination.PaymentModelId = source.PaymentModel?.Id;
            destination.PaymentModelName = source.PaymentModel?.Name;

            //Payment frequency
            destination.PaymentFrequencyId = source.PaymentFreqency?.Id;
            destination.PaymentFrequencyName = source.PaymentFreqency?.Name;

            //Duration
            MapDuration(source, destination);

            //Payment frequency
            destination.OptionExtendId = source.OptionExtend?.Id;
            destination.OptionExtendName = source.OptionExtend?.Name;

            //Termination deadline
            destination.TerminationDeadlineId = source.TerminationDeadline?.Id;
            destination.TerminationDeadlineName = source.TerminationDeadline?.Name;
        }

        private void MapDuration(ItContract source, ItContractOverviewReadModel destination)
        {
            //public string Duration { get; set; }
            //TODO
        }

        private void MapEconomyStreams(ItContract source, ItContractOverviewReadModel destination)
        {
            //public int? AccumulatedAcquisitionCost { get; set; }
            //public int? AccumulatedOperationCost { get; set; }
            //public int? AccumulatedOtherCost { get; set; }
            //public DateTime? LatestAuditDate { get; set; }
            //public int? AuditStatusWhite { get; set; }
            //public int? AuditStatusRed { get; set; }
            //public int? AuditStatusYellow { get; set; }
            //public int? AuditStatusGreen { get; set; }
            //public int? AuditStatusMax { get; set; }
            //TODO
        }

        private void MapSystemUsages(ItContract source, ItContractOverviewReadModel destination)
        {
            //TODO
            //public ICollection<ItContractOverviewReadModelItSystemUsage> ItSystemUsages { get; set; } //used for generating links and filtering IN collection (we can add index since the name can be constrained)
            //public string ItSystemUsagesCsv { get; set; } //Used for sorting AND excel output 
            //public string ItSystemUsagesSystemUuidCsv { get; set; } //Used for sorting AND excel output 
        }

        private void MapDataProcessingAgreements(ItContract source, ItContractOverviewReadModel destination)
        {
            //TODO
            //public ICollection<ItContractOverviewReadModelDataProcessingAgreement> DataProcessingAgreements { get; set; } //used for generating links and filtering IN collection (we can add index since the name can be constrained)
            //public string DataProcessingAgreementsCsv { get; set; } //Used for sorting AND excel output (not filtering since we cannot set a ceiling on length and hence no index)
        }

        private void MapRoleAssignments(ItContract source, ItContractOverviewReadModel destination)
        {
            //TODO
            //public ICollection<ItContractOverviewRoleAssignmentReadModel> RoleAssignments { get; set; }
        }

        private static void PatchSourceRelationshipInformation(ItContract source, ItContractOverviewReadModel destination)
        {
            destination.OrganizationId = source.OrganizationId;
            destination.SourceEntityId = source.Id;
        }
    }
}