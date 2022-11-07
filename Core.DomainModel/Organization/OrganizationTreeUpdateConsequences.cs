using System.Collections.Generic;

namespace Core.DomainModel.Organization
{
    /// <summary>
    /// Describes the consequences of an organization tree update
    /// </summary>
    public class OrganizationTreeUpdateConsequences
    {
        public IEnumerable<OrganizationUnit> DeletedExternalUnitsBeingConvertedToNativeUnits { get; }
        public IEnumerable<OrganizationUnit> DeletedExternalUnitsBeingDeleted { get; }
        public IEnumerable<(ExternalOrganizationUnit unitToAdd, ExternalOrganizationUnit parent)> AddedExternalOrganizationUnits { get; }
        public IEnumerable<(OrganizationUnit affectedUnit, string oldName, string newName)> OrganizationUnitsBeingRenamed { get; }
        public IEnumerable<(OrganizationUnit movedUnit, OrganizationUnit oldParent, ExternalOrganizationUnit newParent)> OrganizationUnitsBeingMoved { get; }

        public OrganizationTreeUpdateConsequences(
            IEnumerable<OrganizationUnit> deletedExternalUnitsBeingConvertedToNativeUnits,
            IEnumerable<OrganizationUnit> deletedExternalUnitsBeingDeleted,
            IEnumerable<(ExternalOrganizationUnit unitToAdd, ExternalOrganizationUnit parent)> addedExternalOrganizationUnits,
            IEnumerable<(OrganizationUnit affectedUnit, string oldName, string newName)> organizationUnitsBeingRenamed,
            IEnumerable<(OrganizationUnit movedUnit, OrganizationUnit oldParent, ExternalOrganizationUnit newParent)> organizationUnitsBeingMoved)
        {
            DeletedExternalUnitsBeingConvertedToNativeUnits = deletedExternalUnitsBeingConvertedToNativeUnits;
            DeletedExternalUnitsBeingDeleted = deletedExternalUnitsBeingDeleted;
            AddedExternalOrganizationUnits = addedExternalOrganizationUnits;
            OrganizationUnitsBeingRenamed = organizationUnitsBeingRenamed;
            OrganizationUnitsBeingMoved = organizationUnitsBeingMoved;
        }
    }
}
