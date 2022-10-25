using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;

namespace Core.DomainModel.Organization
{
    /// <summary>
    /// Describes the consequences of an organization tree update
    /// </summary>
    public class OrganizationTreeUpdateConsequences
    {
        public Maybe<(int? fromSyncDepth, int? toSyncDepth)> ChangedSynchronizationDepth { get; }
        public IEnumerable<OrganizationUnit> DeletedExternalUnitsBeingConvertedToNativeUnits { get; }
        public IEnumerable<(ExternalOrganizationUnit unitToAdd, ExternalOrganizationUnit parent)> AddedExternalOrganizationUnits { get; }
        public IEnumerable<(OrganizationUnit affectedUnit, string oldName, string newName)> OrganizationUnitsBeingRenamed { get; }
        public IEnumerable<(OrganizationUnit movedUnit, OrganizationUnit oldParent, ExternalOrganizationUnit newParent)> OrganizationUnitsBeingMoved { get; }

        public OrganizationTreeUpdateConsequences(
            Maybe<(int? fromSyncDepth, int? toSyncDepth)> changedSynchronizationDepth,
            IEnumerable<OrganizationUnit> deletedExternalUnitsBeingConvertedToNativeUnits,
            IEnumerable<(ExternalOrganizationUnit unitToAdd, ExternalOrganizationUnit parent)> addedExternalOrganizationUnits,
            IEnumerable<(OrganizationUnit affectedUnit, string oldName, string newName)> organizationUnitsBeingRenamed,
            IEnumerable<(OrganizationUnit movedUnit, OrganizationUnit oldParent, ExternalOrganizationUnit newParent)> organizationUnitsBeingMoved)
        {
            ChangedSynchronizationDepth = changedSynchronizationDepth;
            DeletedExternalUnitsBeingConvertedToNativeUnits = deletedExternalUnitsBeingConvertedToNativeUnits.ToList();
            AddedExternalOrganizationUnits = addedExternalOrganizationUnits.ToList();
            OrganizationUnitsBeingRenamed = organizationUnitsBeingRenamed.ToList();
            OrganizationUnitsBeingMoved = organizationUnitsBeingMoved.ToList();
        }
    }
}
