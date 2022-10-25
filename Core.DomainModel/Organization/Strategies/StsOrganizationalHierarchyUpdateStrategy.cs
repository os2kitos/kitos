using Core.Abstractions.Types;
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

        public OrganizationTreeUpdateConsequences ComputeUpdate(ExternalOrganizationUnit root, Maybe<int> levelsIncluded)
        {
            var currentTree = _organization
                .OrgUnits
                .Where(unit => unit.Origin == OrganizationUnitOrigin.STS_Organisation)
                .ToList();

            if (currentTree.Count == 0)
            {
                throw new InvalidOperationException("No organization units from STS Organisation found in the current hierarchy");
            }

            var currentTreeByUuid = currentTree.ToDictionary(x => x.ExternalOriginUuid.GetValueOrDefault());

            var importedTree = root
                .Copy(levelsIncluded)
                .Flatten()
                .ToList();

            var importedTreeByUuid = importedTree.ToDictionary(x => x.Uuid);
            var importedTreeToParent = importedTree
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
                if (importedParent.Uuid != current.Parent?.ExternalOriginUuid.GetValueOrDefault())
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
            var candidatesForRemovalByUuid = currentTreeByUuid
                .Keys
                .Except(importedTreeByUuid.Keys)
                .Select(uuid => currentTreeByUuid[uuid])
                .ToDictionary(x => x.ExternalOriginUuid.GetValueOrDefault());


            /*
             * TODO:Get the sub tree
             * - Remove all items that will be either REMOVED or MOVED away to ANOTHER sub tree
             * - If any org units remain in the subtree except self, it will be conversion
             * - If only the current org unit remains and ANY registrations on that remains THEN it will be a conversion
             */

            //TODO: Maintain a "to be" tree so we can determine if some units are to be removed completely e.g.
            /*
             *TODO: Scenarios
             * - Removed hence if no registrations exist and no remaining registrations in sub tree (to be) and no native kitos units (requires two pass processing)
             * - - If condition above fails, the unit will be converted
             * - [DONE] Renamed
             *
             * - [DONE] Added to existing parent
             * - [DONE] Added to non-existing parent
             *
             * - [DONE] Moved to existing parent
             * - [DONE] Moved to non-existing parent
             */

            //TODO: Import
            throw new System.NotImplementedException();
        }

        public OrganizationTreeUpdateConsequences PerformUpdate(ExternalOrganizationUnit root, Maybe<int> levelsIncluded)
        {
            throw new System.NotImplementedException();
        }
    }
}
