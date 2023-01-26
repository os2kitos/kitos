using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.DomainModel;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItContract.Read;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Shared;
using Core.DomainModel.Users;
using Core.DomainServices.Mapping;
using Core.DomainServices.Model;

namespace Core.DomainServices.Contract
{
    public class ItContractOverviewReadModelUpdate : IReadModelUpdate<ItContract, ItContractOverviewReadModel>
    {
        private readonly IGenericRepository<ItContractOverviewRoleAssignmentReadModel> _contractRoleAssignmentRepository;
        private readonly IGenericRepository<ItContractOverviewReadModelDataProcessingAgreement> _contractDataProcessingAgreementRepository;
        private readonly IGenericRepository<ItContractOverviewReadModelItSystemUsage> _contractSystemUsageReadModelRepository;
        private readonly IGenericRepository<ItContractOverviewReadModelSystemRelation> _relationsRepository;

        public ItContractOverviewReadModelUpdate(
            IGenericRepository<ItContractOverviewRoleAssignmentReadModel> contractRoleAssignmentRepository,
            IGenericRepository<ItContractOverviewReadModelDataProcessingAgreement> contractDataProcessingAgreementRepository,
            IGenericRepository<ItContractOverviewReadModelItSystemUsage> contractSystemUsageReadModelRepository,
            IGenericRepository<ItContractOverviewReadModelSystemRelation> relationsRepository)
        {
            _contractRoleAssignmentRepository = contractRoleAssignmentRepository;
            _contractDataProcessingAgreementRepository = contractDataProcessingAgreementRepository;
            _contractSystemUsageReadModelRepository = contractSystemUsageReadModelRepository;
            _relationsRepository = relationsRepository;
        }

        public void Apply(ItContract source, ItContractOverviewReadModel destination)
        {
            PatchSourceRelationshipInformation(source, destination);

            //Simple properties
            destination.Name = source.Name;
            destination.IsActive = source.IsActive;
            destination.ContractId = source.ItContractId;
            destination.ContractSigner = source.ContractSigner;
            destination.ProcurementInitiated = source.ProcurementInitiated;
            destination.OperationRemunerationBegunDate = source.OperationRemunerationBegun?.Date;
            destination.IrrevocableTo = source.IrrevocableTo?.Date;
            destination.TerminatedAt = source.Terminated?.Date;
            destination.LastEditedAtDate = source.LastChanged.Date;
            destination.LastEditedByUserName = source.LastChangedByUser?.Transform(GetUserFullName);
            destination.LastEditedByUserId = source.LastChangedByUserId;
            destination.Concluded = source.Concluded?.Date;
            destination.ExpirationDate = source.ExpirationDate?.Date;

            //Supplier
            destination.SupplierId = source.Supplier?.Id;
            destination.SupplierName = source.Supplier?.Name;

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
            MapSystemRelations(source, destination);

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

        private void MapSystemRelations(ItContract source, ItContractOverviewReadModel destination)
        {
            destination.NumberOfAssociatedSystemRelations = source.AssociatedSystemRelations.Count;

            var actionContexts = source.AssociatedSystemRelations
                .ComputeMirrorActions(destination.SystemRelations, x => x.Id.ToString(), x => x.RelationId.ToString())
                .ToList();

            foreach (var actionContext in actionContexts)
            {
                switch (actionContext.Action)
                {
                    case EnumerableMirrorExtensions.MirrorAction.AddToDestination:
                        var newSourceRelation = actionContext.SourceValue.Value;
                        var item = new ItContractOverviewReadModelSystemRelation()
                        {
                            RelationId = newSourceRelation.Id,
                            Parent = destination
                        };
                        PatchRelation(item, newSourceRelation);
                        destination.SystemRelations.Add(item);
                        break;
                    case EnumerableMirrorExtensions.MirrorAction.RemoveFromDestination:
                        var relationToBeRemoved = actionContext.DestinationValue.Value;
                        destination.SystemRelations.Remove(relationToBeRemoved);
                        _relationsRepository.Delete(relationToBeRemoved);
                        break;
                    case EnumerableMirrorExtensions.MirrorAction.MergeToDestination:
                        PatchRelation(actionContext.DestinationValue.Value, actionContext.SourceValue.Value);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private static void PatchRelation(ItContractOverviewReadModelSystemRelation item, SystemRelation newSourceRelation)
        {
            item.FromSystemUsageId = newSourceRelation.FromSystemUsageId;
            item.ToSystemUsageId = newSourceRelation.ToSystemUsageId;
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
            //Reset
            destination.AccumulatedAcquisitionCost =
                destination.AccumulatedOperationCost =
                    destination.AccumulatedOtherCost =
                        0;

            destination.LatestAuditDate = null;

            destination.AuditStatusGreen =
                destination.AuditStatusWhite =
                    destination.AuditStatusRed =
                        destination.AuditStatusYellow =
                            0;

            //If any streams, then compute all related columns
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

                    var latestAuditDateBeforeUpdate = destination.LatestAuditDate?.Date;
                    var currentStreamAuditDate = economyStream.AuditDate?.Date;
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

            destination.ItSystemUsagesCsv = string.Join(", ", itSystemUsages.Select(MapSystemName));
            destination.ItSystemUsagesSystemUuidCsv = string.Join(", ", itSystemUsages.Select(x => x.ItSystem.Uuid.ToString("D")));

            var actionContexts = itSystemUsages
                .ComputeMirrorActions
                (
                    destination: destination.ItSystemUsages,
                    computeSourceItemId: s => s.Id.ToString(),
                    computeDestinationItemId: d => d.ItSystemUsageId.ToString()
                )
                .ToList();

            foreach (var actionContext in actionContexts)
            {
                switch (actionContext.Action)
                {
                    case EnumerableMirrorExtensions.MirrorAction.AddToDestination:
                        var newItem = actionContext.SourceValue.Value;
                        var itSystemUsage = new ItContractOverviewReadModelItSystemUsage
                        {
                            ItSystemUsageId = newItem.Id,
                            Parent = destination
                        };
                        PatchItSystemUsage(itSystemUsage, newItem);
                        destination.ItSystemUsages.Add(itSystemUsage);
                        break;
                    case EnumerableMirrorExtensions.MirrorAction.RemoveFromDestination:
                        var itemToRemove = actionContext.DestinationValue.Value;
                        destination.ItSystemUsages.Remove(itemToRemove);
                        _contractSystemUsageReadModelRepository.Delete(itemToRemove);
                        break;
                    case EnumerableMirrorExtensions.MirrorAction.MergeToDestination:
                        var destinationItem = actionContext.DestinationValue.Value;
                        var sourceItem = actionContext.SourceValue.Value;
                        PatchItSystemUsage(destinationItem, sourceItem);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private static string MapSystemName(ItSystemUsage system)
        {
            return system.MapItSystemName();
        }

        private static void PatchItSystemUsage(ItContractOverviewReadModelItSystemUsage readModel, ItSystemUsage newItem)
        {
            readModel.ItSystemUsageName = newItem.ItSystem.Name;
            readModel.ItSystemUsageSystemUuid = newItem.ItSystem.Uuid.ToString("D");
            readModel.ItSystemIsDisabled = newItem.ItSystem.Disabled;
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

            var mirrorActionContexts = dataProcessingAgreements
                .ComputeMirrorActions
                (
                    destination: destination.DataProcessingAgreements,
                    computeSourceItemId: dpr => dpr.Id.ToString(),
                    computeDestinationItemId: dpr => dpr.DataProcessingRegistrationId.ToString()
                )
                .ToList();

            foreach (var actionContext in mirrorActionContexts)
            {
                switch (actionContext.Action)
                {
                    case EnumerableMirrorExtensions.MirrorAction.AddToDestination:
                        var dataProcessingRegistration = actionContext.SourceValue.Value;
                        var dataProcessingAgreement = new ItContractOverviewReadModelDataProcessingAgreement
                        {
                            DataProcessingRegistrationId = dataProcessingRegistration.Id,
                            Parent = destination
                        };
                        PatchDataProcessingRegistration(dataProcessingAgreement, dataProcessingRegistration);
                        destination.DataProcessingAgreements.Add(dataProcessingAgreement);
                        break;
                    case EnumerableMirrorExtensions.MirrorAction.RemoveFromDestination:
                        var itemToRemove = actionContext.DestinationValue.Value;
                        destination.DataProcessingAgreements.Remove(itemToRemove);
                        _contractDataProcessingAgreementRepository.Delete(itemToRemove);
                        break;
                    case EnumerableMirrorExtensions.MirrorAction.MergeToDestination:
                        var destinationItem = actionContext.DestinationValue.Value;
                        var sourceItem = actionContext.SourceValue.Value;
                        PatchDataProcessingRegistration(destinationItem, sourceItem);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private static void PatchDataProcessingRegistration(ItContractOverviewReadModelDataProcessingAgreement destinationItem, DataProcessingRegistration sourceItem)
        {
            destinationItem.DataProcessingRegistrationName = sourceItem.Name; //Update the name
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
            destination.SourceEntityUuid = source.Uuid;
        }
    }
}