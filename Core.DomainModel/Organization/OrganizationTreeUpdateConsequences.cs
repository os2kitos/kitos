using System;
using System.Collections.Generic;

namespace Core.DomainModel.Organization
{
    /// <summary>
    /// Describes the consequences of an organization tree update
    /// </summary>
    public class OrganizationTreeUpdateConsequences
    {
        public IEnumerable<(Guid externalOriginUuid, OrganizationUnit organizationUnit)> DeletedExternalUnitsBeingConvertedToNativeUnits { get; }
        public IEnumerable<(Guid externalOriginUuid, OrganizationUnit organizationUnit)> DeletedExternalUnitsBeingDeleted { get; }
        public IEnumerable<(ExternalOrganizationUnit unitToAdd, ExternalOrganizationUnit parent)> AddedExternalOrganizationUnits { get; }
        public IEnumerable<(OrganizationUnit affectedUnit, string oldName, string newName)> OrganizationUnitsBeingRenamed { get; }
        public IEnumerable<(OrganizationUnit movedUnit, OrganizationUnit oldParent, ExternalOrganizationUnit newParent)> OrganizationUnitsBeingMoved { get; }

        public OrganizationTreeUpdateConsequences(
            IEnumerable<(Guid, OrganizationUnit)> deletedExternalUnitsBeingConvertedToNativeUnits,
            IEnumerable<(Guid, OrganizationUnit)> deletedExternalUnitsBeingDeleted,
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
