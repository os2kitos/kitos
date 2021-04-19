using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.Read;
using Core.DomainModel.Users;
using Core.DomainServices.Model;
using Core.DomainServices.Options;
using Infrastructure.Services.Types;

namespace Core.DomainServices.SystemUsage
{
    public class ItSystemUsageOverviewReadModelUpdate : IReadModelUpdate<ItSystemUsage, ItSystemUsageOverviewReadModel>
    {
        private readonly IOptionsService<ItSystem, BusinessType> _businessTypeService;

        private readonly IGenericRepository<ItSystemUsageOverviewRoleAssignmentReadModel> _roleAssignmentRepository; 
        private readonly IGenericRepository<ItSystemUsageOverviewTaskRefReadModel> _taskRefRepository;

        public ItSystemUsageOverviewReadModelUpdate(
            IGenericRepository<ItSystemUsageOverviewRoleAssignmentReadModel> roleAssignmentRepository,
            IGenericRepository<ItSystemUsageOverviewTaskRefReadModel> taskRefRepository,
            IOptionsService<ItSystem, BusinessType> businessTypeService)
        {
            _roleAssignmentRepository = roleAssignmentRepository;
            _taskRefRepository = taskRefRepository;
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

            PatchParentSystemName(source, destination);
            PatchRoleAssignments(source, destination);
            PatchResponsibleOrganizationUnit(source, destination); 
            PatchItSystemBusinessType(source, destination);
            PatchItSystemRightsHolder(source, destination);
            PatchKLE(source, destination);
            PatchReference(source, destination);
            PatchMainContract(source, destination);
        }

        private static void PatchMainContract(ItSystemUsage source, ItSystemUsageOverviewReadModel destination)
        {
            destination.MainContractId = source.MainContract?.ItContractId;
            destination.MainContractName = source.MainContract?.ItContract.Name;

            destination.MainContractSupplierId = source.MainContract?.ItContract.Supplier?.Id;
            destination.MainContractSupplierName = source.MainContract?.ItContract.Supplier?.Name;
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
                    //Append the assignment if it is not already present
                    taskRef = new ItSystemUsageOverviewTaskRefReadModel
                    {
                        Parent = destination,
                        KLEId = incomingTaskRef.TaskKey,
                        KLEName = incomingTaskRef.Description
                    };
                    destination.ItSystemTaskRefs.Add(taskRef);
                }
            }
            _taskRefRepository.Save();
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
        }

        private void PatchRoleAssignments(ItSystemUsage source, ItSystemUsageOverviewReadModel destination)
        {
            static string CreateRoleKey(int roleId, int userId) => $"R:{roleId}U:{userId}";

            var incomingRights = source.Rights.ToDictionary(x => CreateRoleKey(x.RoleId, x.UserId));

            //Remove rights which were removed
            var assignmentsToBeRemoved =
                destination.RoleAssignments
                    .Where(x => incomingRights.ContainsKey(CreateRoleKey(x.RoleId, x.UserId)) == false).ToList();

            RemoveAssignments(destination, assignmentsToBeRemoved);

            var existingAssignments = destination.RoleAssignments.ToDictionary(x => CreateRoleKey(x.RoleId, x.UserId));
            foreach (var incomingRight in source.Rights.ToList())
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

            _roleAssignmentRepository.Save();
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
            var fullName = user?.GetFullName();
            return fullName?.TrimEnd().Substring(0, Math.Min(fullName.Length, UserConstraints.MaxNameLength));
        }

    }
}
