using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
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

        private static void MapDuration(ItContract source, ItContractOverviewReadModel destination)
        {
            var result = "";

            //public string Duration { get; set; }
            if (source.DurationOngoing)
            {
                result = "Løbende";
            }
            else
            {
                var years = source.DurationYears;
                var months = source.DurationMonths;

                if (years.GetValueOrDefault(0) > 0)
                {
                    result += $"{years} år";
                    if (months > 0)
                    {
                        result += " og ";
                    }
                }
                if (months.GetValueOrDefault(0) > 0)
                {
                    result += $"{months} måned";
                    if (months > 1)
                    {
                        result += "er";
                    }
                }
            }

            destination.Duration = result;
        }

        private void MapEconomyStreams(ItContract source, ItContractOverviewReadModel destination)
        {
            var statuses = new Dictionary<TrafficLight, int>()
            {
                {TrafficLight.Green,0},
                {TrafficLight.Red,0},
                {TrafficLight.Yellow,0},
                {TrafficLight.White,0}
            };
            if (source.ExternEconomyStreams.Any())
            {
                foreach (var economyStream in source.ExternEconomyStreams)
                {
                    destination.AccumulatedAcquisitionCost += economyStream.Acquisition;
                    destination.AccumulatedOperationCost += economyStream.Operation;
                    destination.AccumulatedOtherCost += economyStream.Other;
                    statuses[economyStream.AuditStatus]++;

                    var latestAuditDateBeforeUpdate = destination.LatestAuditDate;
                    var currentStreamAuditDate = economyStream.AuditDate;
                    if (latestAuditDateBeforeUpdate == null)
                    {
                        destination.LatestAuditDate = currentStreamAuditDate;
                    }
                    else if (currentStreamAuditDate != null && latestAuditDateBeforeUpdate.Value < currentStreamAuditDate.Value)
                    {
                        destination.LatestAuditDate = currentStreamAuditDate;
                    }
                }

                destination.AuditStatusGreen = statuses[TrafficLight.Green];
                destination.AuditStatusWhite = statuses[TrafficLight.White];
                destination.AuditStatusRed = statuses[TrafficLight.Red];
                destination.AuditStatusYellow = statuses[TrafficLight.Yellow];
                destination.AuditStatusMax = statuses.Sum(x => x.Value);
            }
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