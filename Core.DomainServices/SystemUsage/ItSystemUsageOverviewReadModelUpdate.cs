using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.Read;
using Core.DomainServices.Model;
using Core.DomainServices.Options;

namespace Core.DomainServices.SystemUsage
{
    public class ItSystemUsageOverviewReadModelUpdate : IReadModelUpdate<ItSystemUsage, ItSystemUsageOverviewReadModel>
    {
        private readonly IOptionsService<ItSystem, BusinessType> _businessTypeService;

        private readonly IGenericRepository<ItSystemUsageOverviewRoleAssignmentReadModel> _roleAssignmentRepository;

        public ItSystemUsageOverviewReadModelUpdate(
            IGenericRepository<ItSystemUsageOverviewRoleAssignmentReadModel> roleAssignmentRepository,
            IOptionsService<ItSystem, BusinessType> businessTypeService)
        {
            _roleAssignmentRepository = roleAssignmentRepository;
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

            PatchParentSystemName(source, destination);
            PatchRoleAssignments(source, destination);
            PatchResponsibleOrganizationUnit(source, destination); 
            PatchItSystemBusinessType(source, destination);
            PatchItSystemRightsHolder(source, destination);
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

                var fullName = incomingRight.User.GetFullName();
                assignment.UserFullName = fullName.TrimEnd().Substring(0, Math.Min(fullName.Length, 100));
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

    }
}
