using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.Extensions;

namespace Core.DomainModel.Organization.Strategies
{
    public class StsOrganizationalHierarchyUpdateStrategy : IExternalOrganizationalHierarchyUpdateStrategy
    {
        private readonly Organization _organization;

        public StsOrganizationalHierarchyUpdateStrategy(Organization organization)
        {
            _organization = organization;
        }

        public OrganizationTreeUpdateConsequences ComputeUpdate(ExternalOrganizationUnit root)
        {
            var currentTreeByUuid = _organization
                .OrgUnits
                .Where(unit => unit.Origin == OrganizationUnitOrigin.STS_Organisation)
                .ToDictionary(x => x.ExternalOriginUuid.GetValueOrDefault());

            if (currentTreeByUuid.Count == 0)
            {
                throw new InvalidOperationException("No organization units from STS Organisation found in the current hierarchy");
            }

            var importedTreeByUuid = root
                .Flatten()
                .ToDictionary(x => x.Uuid);

            var importedTreeToParent = importedTreeByUuid
                .Values
                .SelectMany(parent => parent.Children.Select(child => (child, parent)))
                .ToDictionary(x => x.child.Uuid, x => x.parent);

            importedTreeToParent.Add(root.Uuid, null); //Add the root as that will not be part of the collection

            //Keys in both collections
            var commonKeys = currentTreeByUuid.Keys.Intersect(importedTreeByUuid.Keys).ToList();

            //Compute renames and "change of parents"
            var renamedUnits = new List<(OrganizationUnit current, ExternalOrganizationUnit imported)>();
            var parentChanges = new List<(OrganizationUnit movedUnit, OrganizationUnit oldParent, ExternalOrganizationUnit newParent)>();

            foreach (var commonKey in commonKeys)
            {
                var current = currentTreeByUuid[commonKey];
                var imported = importedTreeByUuid[commonKey];

                //Renames
                if (imported.Name != current.Name)
                {
                    renamedUnits.Add((current, imported));
                }

                //Moving parent
                var importedParent = importedTreeToParent[commonKey];
                if (importedParent?.Uuid != current.Parent?.ExternalOriginUuid.GetValueOrDefault())
                {
                    parentChanges.Add((current, current.Parent, importedParent));
                }
            }

            //Compute additions
            var additions = new List<(ExternalOrganizationUnit unitToAdd, ExternalOrganizationUnit parent)>();
            foreach (var newUnitUuid in importedTreeByUuid.Keys.Except(commonKeys).ToList())
            {
                var imported = importedTreeByUuid[newUnitUuid];
                additions.Add((imported, importedTreeToParent[newUnitUuid]));
            }

            //Compute which of the potential removals that will result in removal and which will result in migration
            var candidatesForRemovalById = currentTreeByUuid
                .Keys
                .Except(commonKeys)
                .Select(uuid => currentTreeByUuid[uuid])
                .ToDictionary(x => x.Id);

            var removedExternalUnitsWhichMustBeConverted = new List<OrganizationUnit>();
            var removedExternalUnitsWhichMustBeRemoved = new List<OrganizationUnit>();

            foreach (var candidateForRemoval in candidatesForRemovalById)
            {
                var organizationUnit = candidateForRemoval.Value;
                var removedSubtreeIds = organizationUnit
                    .FlattenHierarchy()
                    .Select(x=>x.Id)
                    .ToHashSet();

                var partsOfSubtreeWhichAreMoved = parentChanges
                    .Where(x => removedSubtreeIds.Contains(x.movedUnit.Id))
                    .SelectMany(x => x.movedUnit.FlattenHierarchy())
                    .ToList();

                //Remove all "moved" parts of the sub tree
                foreach (var movedUnit in partsOfSubtreeWhichAreMoved)
                {
                    removedSubtreeIds.Remove(movedUnit.Id);
                }

                //Remove all "removed" parts of the sub tree
                bool IsNotCurrentCandidate(KeyValuePair<int, OrganizationUnit> keyValuePair) => keyValuePair.Key != candidateForRemoval.Key;
                var otherRemovals = candidatesForRemovalById.Where(IsNotCurrentCandidate).ToList();
                foreach (var removedItem in otherRemovals)
                {
                    removedSubtreeIds.Remove(removedItem.Key);
                }

                if (removedSubtreeIds.Count != 1)
                {
                    //Anything left except the candidate, then we must convert the unit to a KITOS-unit?
                    removedExternalUnitsWhichMustBeConverted.Add(organizationUnit);
                }
                else if (organizationUnit.IsUsed())
                {
                    //If there is still registrations, we must convert it
                    removedExternalUnitsWhichMustBeConverted.Add(organizationUnit);
                }
                else
                {
                    //Safe to remove since there is no remaining sub tree and no remaining registrations tied to it
                    removedExternalUnitsWhichMustBeRemoved.Add(organizationUnit);
                }
            }

            return new OrganizationTreeUpdateConsequences(
                removedExternalUnitsWhichMustBeConverted,
                removedExternalUnitsWhichMustBeRemoved,
                additions,
                renamedUnits.Select(x => (x.current, x.current.Name, x.imported.Name)).ToList(),
                parentChanges);
        }
      
        public OrganizationTreeUpdateConsequences PerformUpdate(ExternalOrganizationUnit root)
        {
            throw new System.NotImplementedException();
        }
    }
}
