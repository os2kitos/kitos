using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItContract.Read;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Shared;
using Core.DomainModel.Users;
using Core.DomainServices.Model;

namespace Core.DomainServices.Contract
{
    public class ItContractOverviewReadModelUpdate : IReadModelUpdate<ItContract, ItContractOverviewReadModel>
    {
        private readonly IGenericRepository<ItContractOverviewRoleAssignmentReadModel> _contractRoleAssignmentRepository;
        private readonly IGenericRepository<ItContractOverviewReadModelDataProcessingAgreement> _contractDataProcessingAgreementRepository;
        private readonly IGenericRepository<ItContractOverviewReadModelItSystemUsage> _contractSystemUsageReadModelRepository;

        public ItContractOverviewReadModelUpdate(
            IGenericRepository<ItContractOverviewRoleAssignmentReadModel> contractRoleAssignmentRepository,
            IGenericRepository<ItContractOverviewReadModelDataProcessingAgreement> contractDataProcessingAgreementRepository,
            IGenericRepository<ItContractOverviewReadModelItSystemUsage> contractSystemUsageReadModelRepository)
        {
            _contractRoleAssignmentRepository = contractRoleAssignmentRepository;
            _contractDataProcessingAgreementRepository = contractDataProcessingAgreementRepository;
            _contractSystemUsageReadModelRepository = contractSystemUsageReadModelRepository;
        }

        public void Apply(ItContract source, ItContractOverviewReadModel destination)
        {
            PatchSourceRelationshipInformation(source, destination);

            //Simple properties
            destination.Name = source.Name;
            destination.IsActive = source.IsActive;
            destination.ContractId = source.ItContractId;
            destination.SupplierName = source.Supplier?.Name;
            destination.ContractSigner = source.ContractSigner;
            destination.ProcurementInitiated = source.ProcurementInitiated;
            destination.OperationRemunerationBegunDate = source.OperationRemunerationBegun;
            destination.IrrevocableTo = source.IrrevocableTo;
            destination.TerminatedAt = source.Terminated;
            destination.LastEditedAtDate = source.LastChanged;
            destination.LastEditedByUserName = source.LastChangedByUser?.Transform(GetUserFullName);

            //Parent contract
            destination.ParentContractName = source.Parent?.Name;
            destination.ParentContractId = source.Parent?.Id;

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

            //TODO: Option names must contain the "udgået" text or state!....
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

        private static void MapEconomyStreams(ItContract source, ItContractOverviewReadModel destination)
        {
            destination.AccumulatedAcquisitionCost =
                destination.AccumulatedOperationCost =
                    destination.AccumulatedOtherCost =
                        null;

            destination.LatestAuditDate = null;

            destination.AuditStatusGreen =
                destination.AuditStatusWhite =
                    destination.AuditStatusRed =
                        destination.AuditStatusYellow =
                            destination.AuditStatusMax =
                                null;

            if (source.ExternEconomyStreams.Any())
            {
                var statuses = new Dictionary<TrafficLight, int>()
                {
                    {TrafficLight.Green,0},
                    {TrafficLight.Red,0},
                    {TrafficLight.Yellow,0},
                    {TrafficLight.White,0}
                };

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
            var itSystemUsages = source
                .AssociatedSystemUsages
                .Select(x => x.ItSystemUsage)
                .GroupBy(x => x.Id)
                .Select(x => x.First()) //guard against any duplicates
                .ToList();

            //TODO: Extract rendering - this is a duplicate of the dpr rendering
            destination.ItSystemUsagesCsv = string.Join(", ", itSystemUsages.Select(x => (x.ItSystem.Name, x.ItSystem.Disabled)).Select(nameStatus => $"{nameStatus.Name}{(nameStatus.Disabled ? " (Ikke aktivt)" : "")}"));
            destination.ItSystemUsagesSystemUuidCsv = string.Join(", ", itSystemUsages.Select(x => x.ItSystem.Uuid.ToString("D")));

            //TODO: The collection - remember both id,systemName, and disabld state should be in the comparison key and then also in the read model state
        }

        private void MapDataProcessingAgreements(ItContract source, ItContractOverviewReadModel destination)
        {
            var dataProcessingAgreements = source
                .DataProcessingRegistrations
                .Where(x => x.IsAgreementConcluded == YesNoIrrelevantOption.YES)
                .GroupBy(x => x.Id)
                .Select(x => x.First()) //guard against any duplicates
                .ToList();

            //CSV field
            destination.DataProcessingAgreementsCsv = string.Join(", ", dataProcessingAgreements.Select(x => x.Name));

            //TODO: The collection - remember both id and name should be in the key generation for comparision
        }

        private void MapRoleAssignments(ItContract source, ItContractOverviewReadModel destination)
        {
            static string CreateRoleKey(int roleId, int userId) => $"R:{roleId}U:{userId}";

            //ItContracts allows duplicates of role assignments so we group them and only show one of them
            var incomingRights = source
                .Rights
                .GroupBy(x => CreateRoleKey(x.RoleId, x.UserId))
                .ToDictionary(x => x.Key, x => x.First());

            //Remove rights which were removed
            var assignmentsToBeRemoved =
                destination.RoleAssignments
                    .Where(x => incomingRights.ContainsKey(CreateRoleKey(x.RoleId, x.UserId)) == false).ToList();

            RemoveAssignments(destination, assignmentsToBeRemoved);

            var existingAssignments = destination
                .RoleAssignments
                .GroupBy(x => CreateRoleKey(x.RoleId, x.UserId))
                .ToDictionary(x => x.Key, x => x.First());

            foreach (var incomingRight in incomingRights.Values)
            {
                if (!existingAssignments.TryGetValue(CreateRoleKey(incomingRight.RoleId, incomingRight.UserId), out var assignment))
                {
                    //Append the assignment if it is not already present
                    assignment = new ItContractOverviewRoleAssignmentReadModel
                    {
                        Parent = destination,
                        RoleId = incomingRight.RoleId,
                        UserId = incomingRight.UserId
                    };
                    destination.RoleAssignments.Add(assignment);
                }

                assignment.UserFullName = GetUserFullName(incomingRight.User);
                assignment.Email = incomingRight.User.Email;
            }
        }

        private static string GetUserFullName(User user)
        {
            var fullName = user?.GetFullName()?.TrimEnd();
            return fullName?.Substring(0, Math.Min(fullName.Length, UserConstraints.MaxNameLength));
        }

        private void RemoveAssignments(ItContractOverviewReadModel destination, List<ItContractOverviewRoleAssignmentReadModel> assignmentsToBeRemoved)
        {
            assignmentsToBeRemoved.ForEach(assignmentToBeRemoved =>
            {
                destination.RoleAssignments.Remove(assignmentToBeRemoved);
                _contractRoleAssignmentRepository.Delete(assignmentToBeRemoved);
            });
        }

        private static void PatchSourceRelationshipInformation(ItContract source, ItContractOverviewReadModel destination)
        {
            destination.OrganizationId = source.OrganizationId;
            destination.SourceEntityId = source.Id;
        }
    }
}