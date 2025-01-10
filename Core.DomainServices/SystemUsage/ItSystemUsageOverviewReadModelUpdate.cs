using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystem.DataTypes;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.GDPR;
using Core.DomainModel.ItSystemUsage.Read;
using Core.DomainModel.Shared;
using Core.DomainModel.Users;
using Core.DomainServices.Model;
using Core.DomainServices.Options;

namespace Core.DomainServices.SystemUsage
{
    public class ItSystemUsageOverviewReadModelUpdate : IReadModelUpdate<ItSystemUsage, ItSystemUsageOverviewReadModel>
    {
        private readonly IOptionsService<ItSystem, BusinessType> _businessTypeService;
        private readonly IGenericRepository<ItSystemUsageOverviewRelevantOrgUnitReadModel> _relevantOrgUnitsRepository;

        private readonly IGenericRepository<ItSystemUsageOverviewRoleAssignmentReadModel> _roleAssignmentRepository;
        private readonly IGenericRepository<ItSystemUsageOverviewTaskRefReadModel> _taskRefRepository;
        private readonly IGenericRepository<ItSystemUsageOverviewSensitiveDataLevelReadModel> _sensitiveDataLevelRepository;
        private readonly IGenericRepository<ItSystemUsageOverviewArchivePeriodReadModel> _archivePeriodReadModelRepository;
        private readonly IGenericRepository<ItSystemUsageOverviewDataProcessingRegistrationReadModel> _dataProcessingRegistrationReadModelRepository;
        private readonly IGenericRepository<ItSystemUsageOverviewInterfaceReadModel> _dependsOnInterfaceReadModelRepository;
        private readonly IGenericRepository<ItSystemUsageOverviewUsedBySystemUsageReadModel> _itSystemUsageUsedByRelationReadModelRepository;
        private readonly IGenericRepository<ItSystemUsageOverviewUsingSystemUsageReadModel> _itSystemUsageUsingRelationReadModelRepository;
        private readonly IGenericRepository<ItSystemUsageOverviewItContractReadModel> _itSystemUsageOverviewContractReadModelsRepository;

        public ItSystemUsageOverviewReadModelUpdate(
            IGenericRepository<ItSystemUsageOverviewRoleAssignmentReadModel> roleAssignmentRepository,
            IGenericRepository<ItSystemUsageOverviewTaskRefReadModel> taskRefRepository,
            IGenericRepository<ItSystemUsageOverviewSensitiveDataLevelReadModel> sensitiveDataLevelRepository,
            IGenericRepository<ItSystemUsageOverviewArchivePeriodReadModel> archivePeriodReadModelRepository,
            IGenericRepository<ItSystemUsageOverviewDataProcessingRegistrationReadModel> dataProcessingRegistrationReadModelRepository,
            IGenericRepository<ItSystemUsageOverviewInterfaceReadModel> dependsOnInterfaceReadModelRepository,
            IGenericRepository<ItSystemUsageOverviewUsedBySystemUsageReadModel> itSystemUsageUsedByRelationReadModelRepository,
            IGenericRepository<ItSystemUsageOverviewUsingSystemUsageReadModel> itSystemUsageUsingRelationReadModelRepository,
            IOptionsService<ItSystem, BusinessType> businessTypeService,
            IGenericRepository<ItSystemUsageOverviewRelevantOrgUnitReadModel> relevantOrgUnitsRepository, 
            IGenericRepository<ItSystemUsageOverviewItContractReadModel> itSystemUsageOverviewContractReadModelsRepository)
        {
            _roleAssignmentRepository = roleAssignmentRepository;
            _taskRefRepository = taskRefRepository;
            _sensitiveDataLevelRepository = sensitiveDataLevelRepository;
            _archivePeriodReadModelRepository = archivePeriodReadModelRepository;
            _dataProcessingRegistrationReadModelRepository = dataProcessingRegistrationReadModelRepository;
            _dependsOnInterfaceReadModelRepository = dependsOnInterfaceReadModelRepository;
            _itSystemUsageUsedByRelationReadModelRepository = itSystemUsageUsedByRelationReadModelRepository;
            _itSystemUsageUsingRelationReadModelRepository = itSystemUsageUsingRelationReadModelRepository;
            _businessTypeService = businessTypeService;
            _relevantOrgUnitsRepository = relevantOrgUnitsRepository;
            _itSystemUsageOverviewContractReadModelsRepository = itSystemUsageOverviewContractReadModelsRepository;
        }

        public void Apply(ItSystemUsage source, ItSystemUsageOverviewReadModel destination)
        {
            destination.SourceEntityId = source.Id;
            destination.SourceEntityUuid = source.Uuid;
            destination.ExternalSystemUuid = source.ItSystem.ExternalUuid;
            destination.OrganizationId = source.OrganizationId;
            destination.SystemName = source.ItSystem.Name;
            destination.SystemDescription = source.ItSystem.Description;
            destination.ItSystemDisabled = source.ItSystem.Disabled;
            destination.ActiveAccordingToValidityPeriod = source.IsActiveAccordingToDateFields;
            destination.ActiveAccordingToLifeCycle = source.IsActiveAccordingToLifeCycle;
            destination.SystemActive = source.CheckSystemValidity().Result;
            destination.Note = source.Note;
            destination.Version = source.Version;
            destination.LocalCallName = source.LocalCallName;
            destination.LocalSystemId = source.LocalSystemId;
            destination.ItSystemUuid = source.ItSystem.Uuid.ToString("D");
            destination.ObjectOwnerId = source.ObjectOwnerId;
            destination.ObjectOwnerName = GetUserFullName(source.ObjectOwner);
            destination.LastChangedById = source.LastChangedByUserId;
            destination.LastChangedByName = GetUserFullName(source.LastChangedByUser);
            destination.LastChangedAt = source.LastChanged.Date;
            destination.Concluded = source.Concluded?.Date;
            destination.ExpirationDate = source.ExpirationDate?.Date;
            destination.ArchiveDuty = source.ArchiveDuty;
            destination.IsHoldingDocument = source.Registertype.GetValueOrDefault(false);
            destination.LinkToDirectoryName = source.LinkToDirectoryUrlName;
            destination.LinkToDirectoryUrl = source.LinkToDirectoryUrl;
            destination.HostedAt = source.HostedAt.GetValueOrDefault(HostedAt.UNDECIDED);
            destination.UserCount = source.UserCount.GetValueOrDefault(UserCount.UNDECIDED);
            destination.LifeCycleStatus = source.LifeCycleStatus;
            destination.SystemPreviousName = source.ItSystem.PreviousName;

            PatchParentSystemName(source, destination);
            PatchRoleAssignments(source, destination);
            PatchOrganizationUnits(source, destination);
            PatchItSystemBusinessType(source, destination);
            PatchItSystemCategories(source, destination);
            PatchItSystemRightsHolder(source, destination);
            PatchKLE(source, destination);
            PatchReference(source, destination);
            PatchMainContract(source, destination);
            PatchSensitiveDataLevels(source, destination);
            PatchArchivePeriods(source, destination);
            PatchRiskSupervisionDocumentation(source, destination);
            PatchDataProcessingRegistrations(source, destination);
            PatchGeneralPurposeRegistrations(source, destination);
            PatchDependsOnInterfaces(source, destination);
            PatchRelatedItSystemUsages(
                () => new ItSystemUsageOverviewUsedBySystemUsageReadModel(),
                source,
                destination,
                x => x.UsedByRelations,
                x => x.FromSystemUsage,
                csv => destination.IncomingRelatedItSystemUsagesNamesAsCsv = csv,
                x => x.IncomingRelatedItSystemUsages,
                _itSystemUsageUsedByRelationReadModelRepository);
            PatchRelatedItSystemUsages(
                () => new ItSystemUsageOverviewUsingSystemUsageReadModel(),
                source,
                destination,
                x => x.UsageRelations,
                x => x.ToSystemUsage,
                csv => destination.OutgoingRelatedItSystemUsagesNamesAsCsv = csv,
                x => x.OutgoingRelatedItSystemUsages,
                _itSystemUsageUsingRelationReadModelRepository);
            PatchAssociatedContracts(source, destination);
        }

        private void PatchAssociatedContracts(ItSystemUsage source, ItSystemUsageOverviewReadModel destination)
        {
            destination.AssociatedContractsNamesCsv = source
                .Contracts
                .OrderBy(x => x.ItContract.Name)
                .Select(x => x.ItContract.Name)
                .ToList()
                .Transform(orderedContractNames => string.Join(", ", orderedContractNames));

            //Update contract collection by mirroring source into destination and deleting the orphans
            var contractsBefore = destination.AssociatedContracts.ToList();
            source
                .Contracts
                .Select(x => x.ItContract)
                .Select(x => new ItSystemUsageOverviewItContractReadModel
                {
                    ItContractId = x.Id,
                    ItContractName = x.Name,
                    ItContractUuid = x.Uuid
                })
                .MirrorTo(destination.AssociatedContracts, contract =>
                    //Create identity that triggers a "diff" if name changes
                    $"{contract.ItContractUuid}{contract.ItContractName}");
            var removedContracts = contractsBefore.Except(destination.AssociatedContracts).ToList();
            if (removedContracts.Any())
            {
                _itSystemUsageOverviewContractReadModelsRepository.RemoveRange(removedContracts);
            }
        }

        private static void PatchRelatedItSystemUsages<T>(
            Factory<T> readModelFactory,
            ItSystemUsage source,
            ItSystemUsageOverviewReadModel destination,
            Func<ItSystemUsage, ICollection<SystemRelation>> pickSourceCollection,
            Func<SystemRelation, ItSystemUsage> pickUsageFromRelation,
            Action<string> setCsv,
            Func<ItSystemUsageOverviewReadModel, ICollection<T>> pickDestinationCollection,
            IGenericRepository<T> usageRepository)
            where T : class, IItSystemUsageOverviewItSystemUsageReadModel
        {
            var usagesFromSource = pickSourceCollection(source)
                .Select(pickUsageFromRelation)
                .Where(itSystemUsage => itSystemUsage != null)
                .Where(itSystemUsage => itSystemUsage.ItSystem != null)
                .ToList();
            var destinationCollection = pickDestinationCollection(destination);

            setCsv(string.Join(", ",
                usagesFromSource
                .Select(x => x.ItSystem)
                .Select(x => x.Name)));

            static string CreateRelatedItSystemUsageKey(int Id) => $"I:{Id}";

            var usagesFromSourceByKey =
                usagesFromSource
                .GroupBy(itSystemUsage => CreateRelatedItSystemUsageKey(itSystemUsage.Id))
                .ToDictionary(itSystemUsages => itSystemUsages.Key, x => x.First());

            // Remove RelatedItSystemUsages which were removed
            var relatedItSystemUsagesToBeRemoved =
                destinationCollection
                    .Where(x => usagesFromSourceByKey.ContainsKey(CreateRelatedItSystemUsageKey(x.ItSystemUsageId)) == false)
                    .ToList();

            RemoveRelatedItSystemUsages(destination, relatedItSystemUsagesToBeRemoved, usageRepository, pickDestinationCollection);

            var existingRelatedItSystemUsagesByKey = destinationCollection
                .GroupBy(x => CreateRelatedItSystemUsageKey(x.ItSystemUsageId))
                .ToDictionary(x => x.Key, x => x.First());

            foreach (var usageFromSource in usagesFromSource)
            {
                if (!existingRelatedItSystemUsagesByKey.TryGetValue(CreateRelatedItSystemUsageKey(usageFromSource.Id), out var relatedItSystemUsage))
                {
                    //Append the RelatedItSystemUsage if it is not already present
                    relatedItSystemUsage = readModelFactory();
                    relatedItSystemUsage.Parent = destination;
                    destinationCollection.Add(relatedItSystemUsage);
                }
                relatedItSystemUsage.ItSystemUsageId = usageFromSource.Id;
                relatedItSystemUsage.ItSystemUsageUuid = usageFromSource.Uuid;
                relatedItSystemUsage.ItSystemUsageName = usageFromSource.ItSystem?.Name;
            }
        }

        private void PatchDependsOnInterfaces(ItSystemUsage source, ItSystemUsageOverviewReadModel destination)
        {
            var dependsOnInterfaces = source
                .UsageRelations
                .Where(x => x.RelationInterface != null)
                .Select(x => x.RelationInterface)
                .ToList();

            destination.DependsOnInterfacesNamesAsCsv = string.Join(", ", dependsOnInterfaces.Select(x => x.Name));

            static string CreateDependsOnInterfaceKey(int Id) => $"I:{Id}";

            var incomingDependsOnInterfaces = dependsOnInterfaces
                .GroupBy(x => CreateDependsOnInterfaceKey(x.Id))
                .ToDictionary(x => x.Key, x => x.First());

            // Remove DependsOnInterfaces which were removed
            var dependsOnInterfacesToBeRemoved =
                destination.DependsOnInterfaces
                    .Where(x => incomingDependsOnInterfaces.ContainsKey(CreateDependsOnInterfaceKey(x.InterfaceId)) == false).ToList();

            RemoveDependsOnInterfaces(destination, dependsOnInterfacesToBeRemoved);

            var existingDependsOnInterfaces = destination
                .DependsOnInterfaces
                .GroupBy(x => CreateDependsOnInterfaceKey(x.InterfaceId))
                .ToDictionary(x => x.Key, x => x.First());

            foreach (var incomingDependsOnInterface in dependsOnInterfaces.ToList())
            {
                if (!existingDependsOnInterfaces.TryGetValue(CreateDependsOnInterfaceKey(incomingDependsOnInterface.Id), out var dependsOnInterface))
                {
                    //Append the DependsOnInterface if it is not already present
                    dependsOnInterface = new ItSystemUsageOverviewInterfaceReadModel
                    {
                        Parent = destination
                    };
                    destination.DependsOnInterfaces.Add(dependsOnInterface);
                }
                dependsOnInterface.InterfaceId = incomingDependsOnInterface.Id;
                dependsOnInterface.InterfaceUuid = incomingDependsOnInterface.Uuid;
                dependsOnInterface.InterfaceName = incomingDependsOnInterface.Name;
            }
        }

        private static void PatchGeneralPurposeRegistrations(ItSystemUsage source, ItSystemUsageOverviewReadModel destination)
        {
            var generalPurpose = source.GeneralPurpose?.TrimEnd();
            destination.GeneralPurpose = generalPurpose?.Substring(0, Math.Min(generalPurpose.Length, ItSystemUsage.LongProperyMaxLength));
        }

        private void PatchDataProcessingRegistrations(ItSystemUsage source, ItSystemUsageOverviewReadModel destination)
        {
            destination.DataProcessingRegistrationNamesAsCsv = string.Join(", ", source.AssociatedDataProcessingRegistrations.Select(x => x.Name));
            var isAgreementConcludedList = source.AssociatedDataProcessingRegistrations
                .Select(x => x.IsAgreementConcluded)
                .Where(x => x.GetValueOrDefault(YesNoIrrelevantOption.UNDECIDED) != YesNoIrrelevantOption.UNDECIDED)
                .ToList();

            destination.DataProcessingRegistrationsConcludedAsCsv = string.Join(", ", isAgreementConcludedList.Select(x => x.Value.GetReadableName()));

            destination.RiskAssessmentDate = source.riskAssesmentDate;
            destination.PlannedRiskAssessmentDate = source.PlannedRiskAssessmentDate;

            static string CreateDataProcessingRegistrationKey(int Id) => $"I:{Id}";

            var incomingDataProcessingRegistrations = source.AssociatedDataProcessingRegistrations.ToDictionary(x => CreateDataProcessingRegistrationKey(x.Id));

            // Remove DataProcessingRegistrations which were removed
            var dataProcessingRegistrationsToBeRemoved =
                destination.DataProcessingRegistrations
                    .Where(x => incomingDataProcessingRegistrations.ContainsKey(CreateDataProcessingRegistrationKey(x.DataProcessingRegistrationId)) == false).ToList();

            RemoveDataProcessingRegistrations(destination, dataProcessingRegistrationsToBeRemoved);

            var existingDataProcessingRegistrations = destination.DataProcessingRegistrations.ToDictionary(x => CreateDataProcessingRegistrationKey(x.DataProcessingRegistrationId));
            foreach (var incomingDataProcessingRegistration in source.AssociatedDataProcessingRegistrations.ToList())
            {
                if (!existingDataProcessingRegistrations.TryGetValue(CreateDataProcessingRegistrationKey(incomingDataProcessingRegistration.Id), out var dataProcessingRegistration))
                {
                    //Append the DataProcessingRegistration if it is not already present
                    dataProcessingRegistration = new ItSystemUsageOverviewDataProcessingRegistrationReadModel
                    {
                        Parent = destination,

                    };
                    destination.DataProcessingRegistrations.Add(dataProcessingRegistration);
                }

                dataProcessingRegistration.DataProcessingRegistrationId = incomingDataProcessingRegistration.Id;
                dataProcessingRegistration.DataProcessingRegistrationUuid = incomingDataProcessingRegistration.Uuid;
                dataProcessingRegistration.DataProcessingRegistrationName = incomingDataProcessingRegistration.Name;
                dataProcessingRegistration.IsAgreementConcluded = incomingDataProcessingRegistration.IsAgreementConcluded;
            }
        }

        private static void PatchRiskSupervisionDocumentation(ItSystemUsage source, ItSystemUsageOverviewReadModel destination)
        {
            if (source.riskAssessment == DataOptions.YES)
            {
                destination.RiskSupervisionDocumentationName = source.RiskSupervisionDocumentationUrlName;
                destination.RiskSupervisionDocumentationUrl = source.RiskSupervisionDocumentationUrl;
            }
            else
            {
                destination.RiskSupervisionDocumentationName = null;
                destination.RiskSupervisionDocumentationUrl = null;
            }
        }

        private void PatchArchivePeriods(ItSystemUsage source, ItSystemUsageOverviewReadModel destination)
        {
            static string CreateArchivePeriodKey(DateTime startDate, DateTime endDate) => $"S:{startDate}E:{endDate}";

            var incomingArchivePeriods = source
                .ArchivePeriods
                .GroupBy(x => CreateArchivePeriodKey(x.StartDate, x.EndDate))
                .ToDictionary(x => x.Key, x => x.First());

            // Remove ArchivePeriods which were removed
            var archivePeriodsToBeRemoved =
                destination.ArchivePeriods
                    .Where(x => incomingArchivePeriods.ContainsKey(CreateArchivePeriodKey(x.StartDate, x.EndDate)) == false).ToList();

            RemoveArchivePeriods(destination, archivePeriodsToBeRemoved);

            var existingArchivePeriods = destination
                .ArchivePeriods
                .GroupBy(x => CreateArchivePeriodKey(x.StartDate, x.EndDate))
                .ToDictionary(x => x.Key, x => x.First());

            foreach (var incomingArchivePeriod in incomingArchivePeriods.Values.ToList())
            {
                if (!existingArchivePeriods.TryGetValue(CreateArchivePeriodKey(incomingArchivePeriod.StartDate, incomingArchivePeriod.EndDate), out var archivePeriod))
                {
                    //Append the ArchivePeriod if it is not already present
                    archivePeriod = new ItSystemUsageOverviewArchivePeriodReadModel
                    {
                        Parent = destination
                    };
                    destination.ArchivePeriods.Add(archivePeriod);
                }

                archivePeriod.StartDate = incomingArchivePeriod.StartDate;
                archivePeriod.EndDate = incomingArchivePeriod.EndDate;
            }
        }

        private void PatchSensitiveDataLevels(ItSystemUsage source, ItSystemUsageOverviewReadModel destination)
        {
            destination.SensitiveDataLevelsAsCsv = string.Join(", ", source.SensitiveDataLevels.Select(x => x.SensitivityDataLevel.GetReadableName()));

            var incomingSensitiveDataLevels = source.SensitiveDataLevels.Select(x => x.SensitivityDataLevel).ToList();

            // Remove sensitive data levels which were removed
            var sensitiveDataLevelsToBeRemoved = destination.SensitiveDataLevels.Where(x => incomingSensitiveDataLevels.Contains(x.SensitivityDataLevel) == false).ToList();

            RemoveSensitiveDataLevels(destination, sensitiveDataLevelsToBeRemoved);

            var existingSensitiveDataLevels = destination.SensitiveDataLevels.Select(x => x.SensitivityDataLevel).ToList();
            foreach (var incomingSensitiveDataLevel in incomingSensitiveDataLevels)
            {
                if (!existingSensitiveDataLevels.Contains(incomingSensitiveDataLevel))
                {
                    //Append the sensitive data levels if it is not already present
                    var sensitiveDataLevel = new ItSystemUsageOverviewSensitiveDataLevelReadModel
                    {
                        Parent = destination,
                        SensitivityDataLevel = incomingSensitiveDataLevel
                    };
                    destination.SensitiveDataLevels.Add(sensitiveDataLevel);
                }
            }
        }

        private static void PatchMainContract(ItSystemUsage source, ItSystemUsageOverviewReadModel destination)
        {
            destination.MainContractId = source.MainContract?.ItContractId;
            destination.MainContractSupplierId = source.MainContract?.ItContract?.Supplier?.Id;
            destination.MainContractSupplierName = source.MainContract?.ItContract?.Supplier?.Name;
            destination.MainContractIsActive = source.IsActiveAccordingToMainContract;
        }

        private static void PatchReference(ItSystemUsage source, ItSystemUsageOverviewReadModel destination)
        {
            var title = source.Reference?.Title;
            destination.LocalReferenceTitle = title?.Substring(0, Math.Min(title.Length, ItSystemUsageOverviewReadModel.MaxReferenceTitleLenght));
            destination.LocalReferenceUrl = source.Reference?.URL;
            destination.LocalReferenceDocumentId = source.Reference?.ExternalReferenceId;
        }

        private void PatchItSystemBusinessType(ItSystemUsage source, ItSystemUsageOverviewReadModel destination)
        {
            destination.ItSystemBusinessTypeId = source.ItSystem.BusinessType?.Id;
            destination.ItSystemBusinessTypeUuid = source.ItSystem.BusinessType?.Uuid;
            destination.ItSystemBusinessTypeName = GetNameOfItSystemOption(source.ItSystem, source.ItSystem.BusinessType, _businessTypeService);
        }
        
        private void PatchItSystemCategories(ItSystemUsage source, ItSystemUsageOverviewReadModel destination)
        {
            destination.ItSystemCategoriesId = source.ItSystemCategories?.Id;
            destination.ItSystemCategoriesUuid = source.ItSystemCategories?.Uuid;
            destination.ItSystemCategoriesName = source.ItSystemCategories?.Name;
        }

        private static void PatchItSystemRightsHolder(ItSystemUsage source, ItSystemUsageOverviewReadModel destination)
        {
            destination.ItSystemRightsHolderId = source.ItSystem.BelongsTo?.Id;
            destination.ItSystemRightsHolderName = source.ItSystem.BelongsTo?.Name;
        }

        private void PatchKLE(ItSystemUsage source, ItSystemUsageOverviewReadModel destination)
        {
            destination.ItSystemKLEIdsAsCsv = string.Join(", ", source.ItSystem.TaskRefs.Select(x => x.TaskKey));
            destination.ItSystemKLENamesAsCsv = string.Join(", ", source.ItSystem.TaskRefs.Select(x => x.Description));

            static string CreateTaskRefKey(string KLEId) => $"I:{KLEId}";

            var incomingTaskRefs = source.ItSystem.TaskRefs.ToDictionary(x => CreateTaskRefKey(x.TaskKey));

            // Remove taskref which were removed
            var taskRefsToBeRemoved =
                destination.ItSystemTaskRefs
                    .Where(x => incomingTaskRefs.ContainsKey(CreateTaskRefKey(x.KLEId)) == false).ToList();

            RemoveTaskRefs(destination, taskRefsToBeRemoved);

            var existingTaskRefs = destination.ItSystemTaskRefs.ToDictionary(x => CreateTaskRefKey(x.KLEId));
            foreach (var incomingTaskRef in source.ItSystem.TaskRefs.ToList())
            {
                if (!existingTaskRefs.TryGetValue(CreateTaskRefKey(incomingTaskRef.TaskKey), out var taskRef))
                {
                    //Append the taskref if it is not already present
                    taskRef = new ItSystemUsageOverviewTaskRefReadModel
                    {
                        Parent = destination
                    };
                    destination.ItSystemTaskRefs.Add(taskRef);
                }

                taskRef.KLEId = incomingTaskRef.TaskKey;
                taskRef.KLEName = incomingTaskRef.Description;
            }
        }

        private void PatchOrganizationUnits(ItSystemUsage source, ItSystemUsageOverviewReadModel destination)
        {
            PatchResponsibleOrgUnit(source, destination);
            PatchRelevantOrgUnits(source, destination);
        }

        private void PatchRelevantOrgUnits(ItSystemUsage source, ItSystemUsageOverviewReadModel destination)
        {
            //relevant units - ordering field (csv) and lookup field (collection)
            var relevantUnits = source.UsedBy.Select(x => x.OrganizationUnit).ToList();
            destination.RelevantOrganizationUnitNamesAsCsv =
                string.Join(", ", relevantUnits.OrderBy(x => x.Name).Select(x => x.Name).ToList());

            var actionContexts = relevantUnits.ComputeMirrorActions(
                    destination.RelevantOrganizationUnits,
                    incoming => incoming.Uuid.ToString(),
                    existing => existing.OrganizationUnitUuid.ToString())
                .ToList();


            var removed = new List<ItSystemUsageOverviewRelevantOrgUnitReadModel>();
            foreach (var mirrorActionContext in actionContexts)
            {
                switch (mirrorActionContext.Action)
                {
                    case EnumerableMirrorExtensions.MirrorAction.AddToDestination:
                        var newUnitValue = mirrorActionContext.SourceValue.Value;
                        var newUnit = new ItSystemUsageOverviewRelevantOrgUnitReadModel
                        {
                            OrganizationUnitUuid = newUnitValue.Uuid,
                            Parent = destination,
                            OrganizationUnitName = newUnitValue.Name,
                            OrganizationUnitId = newUnitValue.Id
                        };
                        destination.RelevantOrganizationUnits.Add(newUnit);
                        break;
                    case EnumerableMirrorExtensions.MirrorAction.RemoveFromDestination:
                        var toRemove = mirrorActionContext.DestinationValue.Value;
                        destination.RelevantOrganizationUnits.Remove(toRemove);
                        removed.Add(toRemove);
                        break;
                    case EnumerableMirrorExtensions.MirrorAction.MergeToDestination:
                        var destinationValue = mirrorActionContext.DestinationValue.Value;
                        var sourceValue = mirrorActionContext.SourceValue.Value;
                        destinationValue.OrganizationUnitId = sourceValue.Id;
                        destinationValue.OrganizationUnitName = sourceValue.Name;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            _relevantOrgUnitsRepository.RemoveRange(removed);
        }

        private static void PatchResponsibleOrgUnit(ItSystemUsage source, ItSystemUsageOverviewReadModel destination)
        {
            destination.ResponsibleOrganizationUnitId = source.ResponsibleUsage?.OrganizationUnit?.Id;
            destination.ResponsibleOrganizationUnitUuid = source.ResponsibleUsage?.OrganizationUnit?.Uuid;
            destination.ResponsibleOrganizationUnitName = source.ResponsibleUsage?.OrganizationUnit?.Name;
        }

        private static void PatchParentSystemName(ItSystemUsage source, ItSystemUsageOverviewReadModel destination)
        {
            destination.ParentItSystemName = source.ItSystem.Parent?.Name;
            destination.ParentItSystemId = source.ItSystem.Parent?.Id;
            destination.ParentItSystemUuid = source.ItSystem.Parent?.Uuid;
            destination.ParentItSystemDisabled = source.ItSystem.Parent?.Disabled;
        }

        private void PatchRoleAssignments(ItSystemUsage source, ItSystemUsageOverviewReadModel destination)
        {
            static string CreateRoleKey(int roleId, int userId) => $"R:{roleId}U:{userId}";

            //ItSystemusage allows duplicates of role assignments so we group them and only show one of them
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
                    assignment = new ItSystemUsageOverviewRoleAssignmentReadModel
                    {
                        Parent = destination,
                        RoleId = incomingRight.Role.Id,
                        RoleUuid = incomingRight.Role.Uuid,
                        UserId = incomingRight.UserId
                    };
                    destination.RoleAssignments.Add(assignment);
                }

                assignment.UserFullName = GetUserFullName(incomingRight.User);
                assignment.Email = incomingRight.User.Email;
            }
        }

        private void RemoveAssignments(ItSystemUsageOverviewReadModel destination, List<ItSystemUsageOverviewRoleAssignmentReadModel> assignmentsToBeRemoved)
        {
            assignmentsToBeRemoved.ForEach(assignmentToBeRemoved =>
            {
                destination.RoleAssignments.Remove(assignmentToBeRemoved);
                _roleAssignmentRepository.Delete(assignmentToBeRemoved);
            });
        }

        private void RemoveTaskRefs(ItSystemUsageOverviewReadModel destination, List<ItSystemUsageOverviewTaskRefReadModel> taskRefsToBeRemoved)
        {
            taskRefsToBeRemoved.ForEach(taskRefToBeRemoved =>
            {
                destination.ItSystemTaskRefs.Remove(taskRefToBeRemoved);
                _taskRefRepository.Delete(taskRefToBeRemoved);
            });
        }

        private void RemoveSensitiveDataLevels(ItSystemUsageOverviewReadModel destination, List<ItSystemUsageOverviewSensitiveDataLevelReadModel> sensitiveDataLevelsToBeRemoved)
        {
            sensitiveDataLevelsToBeRemoved.ForEach(sensitiveDataLevelToBeRemoved =>
            {
                destination.SensitiveDataLevels.Remove(sensitiveDataLevelToBeRemoved);
                _sensitiveDataLevelRepository.Delete(sensitiveDataLevelToBeRemoved);
            });
        }

        private void RemoveArchivePeriods(ItSystemUsageOverviewReadModel destination, List<ItSystemUsageOverviewArchivePeriodReadModel> archivePeriodsToBeRemoved)
        {
            archivePeriodsToBeRemoved.ForEach(archivePeriodToBeRemoved =>
            {
                destination.ArchivePeriods.Remove(archivePeriodToBeRemoved);
                _archivePeriodReadModelRepository.Delete(archivePeriodToBeRemoved);
            });
        }

        private void RemoveDataProcessingRegistrations(ItSystemUsageOverviewReadModel destination, List<ItSystemUsageOverviewDataProcessingRegistrationReadModel> dataProcessingRegistrationsToBeRemoved)
        {
            dataProcessingRegistrationsToBeRemoved.ForEach(dataProcessingRegistrationToBeRemoved =>
            {
                destination.DataProcessingRegistrations.Remove(dataProcessingRegistrationToBeRemoved);
                _dataProcessingRegistrationReadModelRepository.Delete(dataProcessingRegistrationToBeRemoved);
            });
        }

        private void RemoveDependsOnInterfaces(ItSystemUsageOverviewReadModel destination, List<ItSystemUsageOverviewInterfaceReadModel> dependsOnInterfacesToBeRemoved)
        {
            dependsOnInterfacesToBeRemoved.ForEach(dependsOnInterfaceToBeRemoved =>
            {
                destination.DependsOnInterfaces.Remove(dependsOnInterfaceToBeRemoved);
                _dependsOnInterfaceReadModelRepository.Delete(dependsOnInterfaceToBeRemoved);
            });
        }

        private static void RemoveRelatedItSystemUsages<T>(
            ItSystemUsageOverviewReadModel destination,
            List<T> relatedItSystemUsagesToBeRemoved,
            IGenericRepository<T> repository,
            Func<ItSystemUsageOverviewReadModel, ICollection<T>> pickDestinationCollection
            )
            where T : class, IItSystemUsageOverviewItSystemUsageReadModel
        {
            var destinationCollection = pickDestinationCollection(destination);
            relatedItSystemUsagesToBeRemoved.ForEach(relatedItSystemUsageToBeRemoved =>
            {
                destinationCollection.Remove(relatedItSystemUsageToBeRemoved);
                repository.Delete(relatedItSystemUsageToBeRemoved);
            });
        }

        private static string GetNameOfItSystemOption<TOption>(
            ItSystem parent,
            TOption optionEntity,
            IOptionsService<ItSystem, TOption> service)
            where TOption : OptionEntity<ItSystem>
        {
            if (optionEntity != null)
            {
                var available = service
                    .GetOption(parent.OrganizationId, optionEntity.Id)
                    .Select(x => x.available)
                    .GetValueOrFallback(false);

                return $"{optionEntity.Name}{(available ? string.Empty : " (udgået)")}";
            }

            return null;
        }

        private static string GetUserFullName(User user)
        {
            var fullName = user?.GetFullName()?.TrimEnd();
            return fullName?.Substring(0, Math.Min(fullName.Length, UserConstraints.MaxNameLength));
        }

    }
}
