using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
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

            var importedTreeByUuid = root.ToLookupByUuid();

            var importedTreeToParent = root.ToParentMap(importedTreeByUuid);

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

            var removedExternalUnitsWhichMustBeConverted = new List<(Guid, OrganizationUnit)>();
            var removedExternalUnitsWhichMustBeRemoved = new List<(Guid, OrganizationUnit)>();

            foreach (var candidateForRemoval in candidatesForRemovalById)
            {
                var organizationUnit = candidateForRemoval.Value;
                var removedSubtreeIds = organizationUnit
                    .FlattenHierarchy()
                    .Select(x => x.Id)
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

                var externalOriginUuid = organizationUnit.ExternalOriginUuid.GetValueOrDefault();
                if (removedSubtreeIds.Count != 1)
                {
                    //Anything left except the candidate, then we must convert the unit to a KITOS-unit?
                    removedExternalUnitsWhichMustBeConverted.Add((externalOriginUuid, organizationUnit));
                }
                else if (organizationUnit.IsUsed())
                {
                    //If there is still registrations, we must convert it
                    removedExternalUnitsWhichMustBeConverted.Add((externalOriginUuid, organizationUnit));
                }
                else
                {
                    //Safe to remove since there is no remaining sub tree and no remaining registrations tied to it
                    removedExternalUnitsWhichMustBeRemoved.Add((externalOriginUuid, organizationUnit));
                }
            }

            return new OrganizationTreeUpdateConsequences(
                removedExternalUnitsWhichMustBeConverted,
                removedExternalUnitsWhichMustBeRemoved,
                additions,
                renamedUnits.Select(x => (x.current, x.current.Name, x.imported.Name)).ToList(),
                parentChanges);
        }

        public Result<OrganizationTreeUpdateConsequences, OperationError> PerformUpdate(ExternalOrganizationUnit root)
        {
            var consequences = ComputeUpdate(root);
            var currentTreeByUuid = _organization
                .OrgUnits
                .Where(unit => unit.Origin == OrganizationUnitOrigin.STS_Organisation)
                .ToDictionary(x => x.ExternalOriginUuid.GetValueOrDefault());

            //Renaming
            foreach (var (affectedUnit, _, newName) in consequences.OrganizationUnitsBeingRenamed)
            {
                var nameToUse = newName ?? "";
                if (nameToUse.Length > OrganizationUnit.MaxNameLength)
                {
                    nameToUse = nameToUse.Substring(0, OrganizationUnit.MaxNameLength);
                }
                var updateNameError = affectedUnit.UpdateName(nameToUse);
                if (updateNameError.HasValue)
                {
                    return updateNameError.Value;
                }
            }

            //Conversion to native units
            foreach (var unitToNativeUnit in consequences.DeletedExternalUnitsBeingConvertedToNativeUnits)
            {
                unitToNativeUnit.organizationUnit.ConvertToNativeKitosUnit();
            }

            //Addition of new units
            foreach (var (unitToAdd, parent) in OrderByParentToLeaf(root, consequences.AddedExternalOrganizationUnits))
            {
                if (currentTreeByUuid.TryGetValue(parent.Uuid, out var parentUnit))
                {
                    var newUnit = unitToAdd.ToOrganizationUnit(OrganizationUnitOrigin.STS_Organisation, _organization, false);

                    var addOrgUnitError = _organization.AddOrganizationUnit(newUnit, parentUnit);
                    if (addOrgUnitError.HasValue)
                    {
                        return addOrgUnitError.Value;
                    }

                    currentTreeByUuid.Add(unitToAdd.Uuid, newUnit);
                }
                else
                {
                    return new OperationError($"Parent unit with external uuid {parent.Uuid} could not be found", OperationFailure.BadInput);
                }
            }

            //Relocation of existing units
            var processingQueue = new Queue<(OrganizationUnit movedUnit, OrganizationUnit oldParent, ExternalOrganizationUnit newParent)>(consequences.OrganizationUnitsBeingMoved);
            while (processingQueue.Any())
            {
                var (movedUnit, oldParent, newParent) = processingQueue.Dequeue();
                if (!currentTreeByUuid.TryGetValue(oldParent.ExternalOriginUuid.GetValueOrDefault(), out var oldParentUnit))
                {
                    return new OperationError($"Old parent unit with uuid {oldParent.Uuid} could not be found", OperationFailure.BadInput);
                }

                if (!currentTreeByUuid.TryGetValue(newParent.Uuid, out var newParentUnit))
                {
                    return new OperationError($"New parent unit with external uuid {newParent.Uuid} could not be found", OperationFailure.BadInput);

                }

                if (movedUnit.SearchSubTree(unit => unit.ExternalOriginUuid.GetValueOrDefault() == newParent.Uuid).HasValue)
                {
                    //Wait while the sub tree is processed so that we don't break relocation rules and not lose any retained child relations
                    processingQueue.Enqueue((movedUnit, oldParent, newParent));
                }
                else
                {
                    var relocationError = _organization.RelocateOrganizationUnit(movedUnit, oldParentUnit, newParentUnit, true);
                    if (relocationError.HasValue)
                    {
                        return relocationError.Value;
                    }
                }
            }

            //Deletion of units
            foreach (var externalUnitToDelete in OrderUnitsToDeleteByLeafToParent(_organization.GetRoot(), consequences.DeletedExternalUnitsBeingDeleted.Select(x => x.organizationUnit).ToList()))
            {
                externalUnitToDelete.ConvertToNativeKitosUnit(); //Convert to KITOS unit before deleting it (external units cannot be deleted)
                var deleteOrganizationUnitError = _organization.DeleteOrganizationUnit(externalUnitToDelete);
                if (deleteOrganizationUnitError.HasValue)
                {
                    return deleteOrganizationUnitError.Value;
                }
            }

            return consequences;
        }

        private static IEnumerable<OrganizationUnit> OrderUnitsToDeleteByLeafToParent(OrganizationUnit root, IEnumerable<OrganizationUnit> deletedUnits)
        {
            var unitsToDelete = deletedUnits.ToList();
            var relevantIds = unitsToDelete.Select(x => x.Uuid).ToHashSet();

            var ordering = CreateUuidToIndexMap(root.FlattenHierarchy().Select(x => x.Uuid).ToList(), relevantIds);

            //Make sure leafs are added before children
            return unitsToDelete.OrderByDescending(unitToDelete => ordering[unitToDelete.Uuid]).ToList();
        }

        private static IEnumerable<(ExternalOrganizationUnit unitToAdd, ExternalOrganizationUnit parent)> OrderByParentToLeaf(ExternalOrganizationUnit externalRoot, IEnumerable<(ExternalOrganizationUnit unitToAdd, ExternalOrganizationUnit parent)> addedUnits)
        {
            var unitsToAdd = addedUnits.ToList();
            var relevantIds = unitsToAdd.SelectMany(x => new[] { x.parent.Uuid, x.unitToAdd.Uuid }).ToHashSet();
            var ordering = CreateUuidToIndexMap(externalRoot.Flatten().Select(x => x.Uuid).ToList(), relevantIds);

            //Make sure parents are added before children
            return unitsToAdd.OrderBy(unitToAdd => ordering[unitToAdd.unitToAdd.Uuid]).ToList();
        }

        private static Dictionary<Guid, int> CreateUuidToIndexMap(IEnumerable<Guid> flattenedHierarchy, HashSet<Guid> relevantIds)
        {
            var ordering = flattenedHierarchy
                //Select only the parts that we care about
                .Where(relevantIds.Contains)
                //Find the ordering key of those units
                .Select((uuid, index) => new { uuid, index })
                //Create the lookup
                .ToDictionary(x => x.uuid, x => x.index);
            return ordering;
        }
    }
}
