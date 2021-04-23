﻿using System;
using System.Collections.Generic;
using System.Linq;
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

        private readonly IGenericRepository<ItSystemUsageOverviewRoleAssignmentReadModel> _roleAssignmentRepository;
        private readonly IGenericRepository<ItSystemUsageOverviewTaskRefReadModel> _taskRefRepository;
        private readonly IGenericRepository<ItSystemUsageOverviewSensitiveDataLevelReadModel> _sensitiveDataLevelRepository;
        private readonly IGenericRepository<ItSystemUsageOverviewItProjectReadModel> _itProjectReadModelRepository;
        private readonly IGenericRepository<ItSystemUsageOverviewArchivePeriodReadModel> _archivePeriodReadModelRepository;
        private readonly IGenericRepository<ItSystemUsageOverviewDataProcessingRegistrationReadModel> _dataProcessingRegistrationReadModelRepository;
        private readonly IGenericRepository<ItSystemUsageOverviewInterfaceReadModel> _appliedInterfaceReadModelRepository;
        private readonly IGenericRepository<ItSystemUsageOverviewItSystemUsageReadModel> _itSystemUsageReadModelRepository;

        public ItSystemUsageOverviewReadModelUpdate(
            IGenericRepository<ItSystemUsageOverviewRoleAssignmentReadModel> roleAssignmentRepository,
            IGenericRepository<ItSystemUsageOverviewTaskRefReadModel> taskRefRepository,
            IGenericRepository<ItSystemUsageOverviewSensitiveDataLevelReadModel> sensitiveDataLevelRepository,
            IGenericRepository<ItSystemUsageOverviewItProjectReadModel> itProjectReadModelRepository,
            IGenericRepository<ItSystemUsageOverviewArchivePeriodReadModel> archivePeriodReadModelRepository,
            IGenericRepository<ItSystemUsageOverviewDataProcessingRegistrationReadModel> dataProcessingRegistrationReadModelRepository,
            IGenericRepository<ItSystemUsageOverviewInterfaceReadModel> appliedInterfaceReadModelRepository,
            IGenericRepository<ItSystemUsageOverviewItSystemUsageReadModel> itSystemUsageReadModelRepository,
            IOptionsService<ItSystem, BusinessType> businessTypeService)
        {
            _roleAssignmentRepository = roleAssignmentRepository;
            _taskRefRepository = taskRefRepository;
            _sensitiveDataLevelRepository = sensitiveDataLevelRepository;
            _itProjectReadModelRepository = itProjectReadModelRepository;
            _archivePeriodReadModelRepository = archivePeriodReadModelRepository;
            _dataProcessingRegistrationReadModelRepository = dataProcessingRegistrationReadModelRepository;
            _appliedInterfaceReadModelRepository = appliedInterfaceReadModelRepository;
            _itSystemUsageReadModelRepository = itSystemUsageReadModelRepository;
            _businessTypeService = businessTypeService;
        }

        public void Apply(ItSystemUsage source, ItSystemUsageOverviewReadModel destination)
        {
            destination.SourceEntityId = source.Id;
            destination.OrganizationId = source.OrganizationId;
            destination.Name = source.ItSystem.Name;
            destination.ItSystemDisabled = source.ItSystem.Disabled;
            destination.IsActive = source.IsActive;
            destination.Version = source.Version;
            destination.LocalCallName = source.LocalCallName;
            destination.LocalSystemId = source.LocalSystemId;
            destination.ItSystemUuid = source.ItSystem.Uuid.ToString("D");
            destination.ObjectOwnerId = source.ObjectOwnerId;
            destination.ObjectOwnerName = GetUserFullName(source.ObjectOwner);
            destination.LastChangedById = source.LastChangedByUserId;
            destination.LastChangedByName = GetUserFullName(source.LastChangedByUser);
            destination.LastChanged = source.LastChanged;
            destination.Concluded = source.Concluded;
            destination.ArchiveDuty = source.ArchiveDuty;
            destination.IsHoldingDocument = source.Registertype.GetValueOrDefault(false);
            destination.LinkToDirectoryName = source.LinkToDirectoryUrlName;
            destination.LinkToDirectoryUrl = source.LinkToDirectoryUrl;
            destination.HostedAt = source.HostedAt.GetValueOrDefault(HostedAt.UNDECIDED);

            PatchParentSystemName(source, destination);
            PatchRoleAssignments(source, destination);
            PatchResponsibleOrganizationUnit(source, destination);
            PatchItSystemBusinessType(source, destination);
            PatchItSystemRightsHolder(source, destination);
            PatchKLE(source, destination);
            PatchReference(source, destination);
            PatchMainContract(source, destination);
            PatchSensitiveDataLevels(source, destination);
            PatchItProjects(source, destination);
            PatchArchivePeriods(source, destination);
            PatchRiskSupervisionDocumentation(source, destination);
            PatchDataProcessingRegistrations(source, destination);
            PatchGeneralPurposeRegistrations(source, destination);
            PatchAppliedInterfaces(source, destination);
            PatchRelatedItSystemUsages(source, destination);
        }

        private void PatchRelatedItSystemUsages(ItSystemUsage source, ItSystemUsageOverviewReadModel destination)
        {
            destination.IncomingRelatedItSystemUsagesNamesAsCsv = string.Join(", ", 
                source
                .UsedByRelations
                .Where(x => x.FromSystemUsage != null)
                .Select(x => x.FromSystemUsage)
                .Where(x => x.ItSystem != null)
                .Select(x => x.ItSystem)
                .Select(x => x.Name));

            static string CreateRelatedItSystemUsageKey(int Id, string Name) => $"I:{Id}N:{Name}";

            var incomingRelatedItSystemUsages = source
                .UsedByRelations
                .Where(x => x.FromSystemUsage != null)
                .Select(x => x.FromSystemUsage)
                .Where(x => x.ItSystem != null)
                .ToDictionary(x => CreateRelatedItSystemUsageKey(x.Id, x.ItSystem.Name));

            // Remove RelatedItSystemUsages which were removed
            var relatedItSystemUsagesToBeRemoved =
                destination.IncomingRelatedItSystemUsages
                    .Where(x => incomingRelatedItSystemUsages.ContainsKey(CreateRelatedItSystemUsageKey(x.ItSystemUsageId, x.ItSystemUsageName)) == false).ToList();

            RemoveRelatedItSystemUsages(destination, relatedItSystemUsagesToBeRemoved);

            var existingRelatedItSystemUsages = destination.IncomingRelatedItSystemUsages.ToDictionary(x => CreateRelatedItSystemUsageKey(x.ItSystemUsageId, x.ItSystemUsageName));
            foreach (var incomingRelatedItSystemUsage in source.UsedByRelations.Where(x => x.FromSystemUsage != null).Select(x => x.FromSystemUsage).Where(x => x.ItSystem != null).ToList())
            {
                if (!existingRelatedItSystemUsages.TryGetValue(CreateRelatedItSystemUsageKey(incomingRelatedItSystemUsage.Id, incomingRelatedItSystemUsage.ItSystem.Name), out var relatedItSystemUsage))
                {
                    //Append the RelatedItSystemUsage if it is not already present
                    relatedItSystemUsage = new ItSystemUsageOverviewItSystemUsageReadModel
                    {
                        Parent = destination,
                        ItSystemUsageId = incomingRelatedItSystemUsage.Id,
                        ItSystemUsageName = incomingRelatedItSystemUsage.ItSystem?.Name
                    };
                    destination.IncomingRelatedItSystemUsages.Add(relatedItSystemUsage);
                }
            }
        }

        private void PatchAppliedInterfaces(ItSystemUsage source, ItSystemUsageOverviewReadModel destination)
        {
            destination.AppliedInterfacesNamesAsCsv = string.Join(", ", 
                source
                .UsageRelations
                .Where(x => x.RelationInterface != null)
                .Select(x => x.RelationInterface)
                .Select(x => x.Name));

            static string CreateAppliedInterfaceKey(int Id, string Name) => $"I:{Id}N:{Name}";

            var incomingAppliedInterfaces = source
                .UsageRelations
                .Where(x => x.RelationInterface != null)
                .Select(x => x.RelationInterface)
                .ToDictionary(x => CreateAppliedInterfaceKey(x.Id, x.Name));

            // Remove AppliedInterfaces which were removed
            var appliedInterfacesToBeRemoved =
                destination.AppliedInterfaces
                    .Where(x => incomingAppliedInterfaces.ContainsKey(CreateAppliedInterfaceKey(x.InterfaceId, x.InterfaceName)) == false).ToList();

            RemoveAppliedInterfaces(destination, appliedInterfacesToBeRemoved);

            var existingAppliedInterfaces = destination.AppliedInterfaces.ToDictionary(x => CreateAppliedInterfaceKey(x.InterfaceId, x.InterfaceName));
            foreach (var incomingAppliedInterface in source.UsageRelations.Where(x => x.RelationInterface != null).Select(x => x.RelationInterface).ToList())
            {
                if (!existingAppliedInterfaces.TryGetValue(CreateAppliedInterfaceKey(incomingAppliedInterface.Id, incomingAppliedInterface.Name), out var appliedInterface))
                {
                    //Append the AppliedInterface if it is not already present
                    appliedInterface = new ItSystemUsageOverviewInterfaceReadModel
                    {
                        Parent = destination,
                        InterfaceId = incomingAppliedInterface.Id,
                        InterfaceName = incomingAppliedInterface.Name
                    };
                    destination.AppliedInterfaces.Add(appliedInterface);
                }
            }
        }

        private void PatchGeneralPurposeRegistrations(ItSystemUsage source, ItSystemUsageOverviewReadModel destination)
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

            static string CreateDataProcessingRegistrationKey(int Id, string Name, YesNoIrrelevantOption? IsAgreementConcluded) => $"I:{Id}N:{Name}C:{IsAgreementConcluded}";

            var incomingDataProcessingRegistrations = source.AssociatedDataProcessingRegistrations.ToDictionary(x => CreateDataProcessingRegistrationKey(x.Id, x.Name, x.IsAgreementConcluded));

            // Remove DataProcessingRegistrations which were removed
            var dataProcessingRegistrationsToBeRemoved =
                destination.DataProcessingRegistrations
                    .Where(x => incomingDataProcessingRegistrations.ContainsKey(CreateDataProcessingRegistrationKey(x.DataProcessingRegistrationId, x.DataProcessingRegistrationName, x.IsAgreementConcluded)) == false).ToList();

            RemoveDataProcessingRegistrations(destination, dataProcessingRegistrationsToBeRemoved);

            var existingDataProcessingRegistrations = destination.DataProcessingRegistrations.ToDictionary(x => CreateDataProcessingRegistrationKey(x.DataProcessingRegistrationId, x.DataProcessingRegistrationName, x.IsAgreementConcluded));
            foreach (var incomingDataProcessingRegistration in source.AssociatedDataProcessingRegistrations.ToList())
            {
                if (!existingDataProcessingRegistrations.TryGetValue(CreateDataProcessingRegistrationKey(incomingDataProcessingRegistration.Id, incomingDataProcessingRegistration.Name, incomingDataProcessingRegistration.IsAgreementConcluded), out var dataProcessingRegistration))
                {
                    //Append the DataProcessingRegistration if it is not already present
                    dataProcessingRegistration = new ItSystemUsageOverviewDataProcessingRegistrationReadModel
                    {
                        Parent = destination,
                        DataProcessingRegistrationId = incomingDataProcessingRegistration.Id,
                        DataProcessingRegistrationName = incomingDataProcessingRegistration.Name,
                        IsAgreementConcluded = incomingDataProcessingRegistration.IsAgreementConcluded
                    };
                    destination.DataProcessingRegistrations.Add(dataProcessingRegistration);
                }
            }
        }

        private void PatchRiskSupervisionDocumentation(ItSystemUsage source, ItSystemUsageOverviewReadModel destination)
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
                        Parent = destination,
                        StartDate = incomingArchivePeriod.StartDate,
                        EndDate = incomingArchivePeriod.EndDate
                    };
                    destination.ArchivePeriods.Add(archivePeriod);
                }
            }
        }

        private void PatchItProjects(ItSystemUsage source, ItSystemUsageOverviewReadModel destination)
        {
            destination.ItProjectNamesAsCsv = string.Join(", ", source.ItProjects.Select(x => x.Name));

            static string CreateItProjectKey(int Id, string Name) => $"I:{Id}N:{Name}";

            var incomingItProjects = source.ItProjects.ToDictionary(x => CreateItProjectKey(x.Id, x.Name));

            // Remove It Projects which were removed
            var itProjectsToBeRemoved =
                destination.ItProjects
                    .Where(x => incomingItProjects.ContainsKey(CreateItProjectKey(x.ItProjectId, x.ItProjectName)) == false).ToList();

            RemoveItProjects(destination, itProjectsToBeRemoved);

            var existingItProjects = destination.ItProjects.ToDictionary(x => CreateItProjectKey(x.ItProjectId, x.ItProjectName));
            foreach (var incomingItProject in source.ItProjects.ToList())
            {
                if (!existingItProjects.TryGetValue(CreateItProjectKey(incomingItProject.Id, incomingItProject.Name), out var itProject))
                {
                    //Append the sensitive data levels if it is not already present
                    itProject = new ItSystemUsageOverviewItProjectReadModel
                    {
                        Parent = destination,
                        ItProjectId = incomingItProject.Id,
                        ItProjectName = incomingItProject.Name
                    };
                    destination.ItProjects.Add(itProject);
                }
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
            destination.MainContractIsActive = source.MainContract?.ItContract?.IsActive;
            destination.HasMainContract = source.MainContract?.ItContract != null;
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
            destination.ItSystemBusinessTypeName = GetNameOfItSystemOption(source.ItSystem, source.ItSystem.BusinessType, _businessTypeService);
        }
        private void PatchItSystemRightsHolder(ItSystemUsage source, ItSystemUsageOverviewReadModel destination)
        {
            destination.ItSystemRightsHolderId = source.ItSystem.BelongsTo?.Id;
            destination.ItSystemRightsHolderName = source.ItSystem.BelongsTo?.Name;
        }

        private void PatchKLE(ItSystemUsage source, ItSystemUsageOverviewReadModel destination)
        {
            destination.ItSystemKLEIdsAsCsv = string.Join(", ", source.ItSystem.TaskRefs.Select(x => x.TaskKey));
            destination.ItSystemKLENamesAsCsv = string.Join(", ", source.ItSystem.TaskRefs.Select(x => x.Description));

            static string CreateTaskRefKey(string KLEId, string KLEName) => $"I:{KLEId}N:{KLEName}";

            var incomingTaskRefs = source.ItSystem.TaskRefs.ToDictionary(x => CreateTaskRefKey(x.TaskKey, x.Description));

            // Remove taskref which were removed
            var taskRefsToBeRemoved =
                destination.ItSystemTaskRefs
                    .Where(x => incomingTaskRefs.ContainsKey(CreateTaskRefKey(x.KLEId, x.KLEName)) == false).ToList();

            RemoveTaskRefs(destination, taskRefsToBeRemoved);

            var existingTaskRefs = destination.ItSystemTaskRefs.ToDictionary(x => CreateTaskRefKey(x.KLEId, x.KLEName));
            foreach (var incomingTaskRef in source.ItSystem.TaskRefs.ToList())
            {
                if (!existingTaskRefs.TryGetValue(CreateTaskRefKey(incomingTaskRef.TaskKey, incomingTaskRef.Description), out var taskRef))
                {
                    //Append the taskref if it is not already present
                    taskRef = new ItSystemUsageOverviewTaskRefReadModel
                    {
                        Parent = destination,
                        KLEId = incomingTaskRef.TaskKey,
                        KLEName = incomingTaskRef.Description
                    };
                    destination.ItSystemTaskRefs.Add(taskRef);
                }
            }
        }

        private void PatchResponsibleOrganizationUnit(ItSystemUsage source, ItSystemUsageOverviewReadModel destination)
        {
            destination.ResponsibleOrganizationUnitId = source.ResponsibleUsage?.OrganizationUnit?.Id;
            destination.ResponsibleOrganizationUnitName = source.ResponsibleUsage?.OrganizationUnit?.Name;
        }

        private static void PatchParentSystemName(ItSystemUsage source, ItSystemUsageOverviewReadModel destination)
        {
            destination.ParentItSystemName = source.ItSystem.Parent?.Name;
            destination.ParentItSystemId = source.ItSystem.Parent?.Id;
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
                        RoleId = incomingRight.RoleId,
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

        private void RemoveItProjects(ItSystemUsageOverviewReadModel destination, List<ItSystemUsageOverviewItProjectReadModel> itProjectsToBeRemoved)
        {
            itProjectsToBeRemoved.ForEach(itProjectToBeRemoved =>
            {
                destination.ItProjects.Remove(itProjectToBeRemoved);
                _itProjectReadModelRepository.Delete(itProjectToBeRemoved);
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

        private void RemoveAppliedInterfaces(ItSystemUsageOverviewReadModel destination, List<ItSystemUsageOverviewInterfaceReadModel> appliedInterfacesToBeRemoved)
        {
            appliedInterfacesToBeRemoved.ForEach(appliedInterfaceToBeRemoved =>
            {
                destination.AppliedInterfaces.Remove(appliedInterfaceToBeRemoved);
                _appliedInterfaceReadModelRepository.Delete(appliedInterfaceToBeRemoved);
            });
        }

        private void RemoveRelatedItSystemUsages(ItSystemUsageOverviewReadModel destination, List<ItSystemUsageOverviewItSystemUsageReadModel> relatedItSystemUsagesToBeRemoved)
        {
            relatedItSystemUsagesToBeRemoved.ForEach(relatedItSystemUsageToBeRemoved =>
            {
                destination.IncomingRelatedItSystemUsages.Remove(relatedItSystemUsageToBeRemoved);
                _itSystemUsageReadModelRepository.Delete(relatedItSystemUsageToBeRemoved);
            });
        }

        private string GetNameOfItSystemOption<TOption>(
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
