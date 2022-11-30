using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.DomainModel.Extensions;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainModel.Organization.Strategies;
using Tests.Toolkit.Extensions;
using Tests.Toolkit.Patterns;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Tests.Unit.Core.Model.Strategies
{
    public class StsOrganizationalHierarchyUpdateStrategyTest : WithAutoFixture
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly StsOrganizationalHierarchyUpdateStrategy _sut;
        private readonly Organization _organization;
        private int _nextOrgUnitId;

        public StsOrganizationalHierarchyUpdateStrategyTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _organization = new Organization();
            _sut = new StsOrganizationalHierarchyUpdateStrategy(_organization);
            _nextOrgUnitId = 0;
        }

        private int GetNewOrgUnitId() => _nextOrgUnitId++;

        private void PrepareConnectedOrganization(OrganizationUnit predefinedRoot = null)
        {
            _organization.StsOrganizationConnection = new StsOrganizationConnection
            {
                Connected = true,
                Organization = _organization
            };

            var organizationUnit = predefinedRoot ?? CreateOrganizationUnit
            (
                OrganizationUnitOrigin.STS_Organisation, new[]
                {
                    CreateOrganizationUnit(OrganizationUnitOrigin.Kitos),
                    CreateOrganizationUnit(OrganizationUnitOrigin.STS_Organisation,new []
                    {
                        CreateOrganizationUnit(OrganizationUnitOrigin.STS_Organisation)
                    }),
                    CreateOrganizationUnit(OrganizationUnitOrigin.STS_Organisation, new[]
                    {
                        CreateOrganizationUnit(OrganizationUnitOrigin.STS_Organisation),
                        CreateOrganizationUnit(OrganizationUnitOrigin.Kitos)

                    })
                });

            foreach (var unit in organizationUnit.FlattenHierarchy())
            {
                _organization.OrgUnits.Add(unit);
            }
        }

        private OrganizationUnit CreateOrganizationUnit(OrganizationUnitOrigin origin, IEnumerable<OrganizationUnit> children = null)
        {
            var unit = new OrganizationUnit
            {
                Id = GetNewOrgUnitId(),
                Name = A<string>(),
                Origin = origin,
                ExternalOriginUuid = origin == OrganizationUnitOrigin.STS_Organisation ? A<Guid>() : null,
                Organization = _organization
            };

            if (children != null)
            {
                foreach (var child in children)
                {
                    child.Parent = unit;
                    unit.Children.Add(child);
                }
            }
            return unit;
        }

        [Fact, Description("Since it is an update strategy it is a programmers error to invoke it on an unconnected organization")]
        public void ComputeUpdate_Throws_If_Organization_Is_Not_Connected()
        {
            //Arrange
            var externalOrganizationUnit = new ExternalOrganizationUnit(A<Guid>(), A<string>(), new Dictionary<string, string>(), Array.Empty<ExternalOrganizationUnit>());

            //Act + Assert
            Assert.Throws<InvalidOperationException>(() => _sut.ComputeUpdate(externalOrganizationUnit));
        }

        [Fact, Description("Ensures that change sets that contain no changes will not impact kitos")]
        public void ComputeUpdate_Detects_No_External_Changes()
        {
            //Arrange
            PrepareConnectedOrganization();
            var root = _organization.GetRoot();

            var externalTree = ConvertToExternalTree(root);

            //Act
            var consequences = _sut.ComputeUpdate(externalTree);

            //Assert
            Assert.Empty(consequences.AddedExternalOrganizationUnits);
            Assert.Empty(consequences.DeletedExternalUnitsBeingConvertedToNativeUnits);
            Assert.Empty(consequences.DeletedExternalUnitsBeingDeleted);
            Assert.Empty(consequences.OrganizationUnitsBeingMoved);
            Assert.Empty(consequences.OrganizationUnitsBeingRenamed);
        }

        [Fact, Description("Verifies that additions in the external hierarchy is detected correctly")]
        public void ComputeUpdate_Detects_New_OrganizationUnits()
        {
            //Arrange
            var (_, externalTree, randomParentOfNewSubTree, expectedSubTree, expectedChild, expectedNewUnits) = CreateTreeWithNewOrganizationUnits();

            //Act
            var consequences = _sut.ComputeUpdate(externalTree);

            //Assert
            AssertNewUnitsWereDetected(consequences, expectedNewUnits, expectedSubTree, randomParentOfNewSubTree, expectedChild);
        }

        [Fact]
        public void ComputeUpdate_Detects_Renamed_OrganizationUnits()
        {
            //Arrange
            var (_, externalTree, randomItemToRename, expectedOldName, expectedNewName) = CreateTreeWithRenamedOrganizationUnits();

            //Act
            var consequences = _sut.ComputeUpdate(externalTree);

            //Assert
            AssertUnitsToRenameWereDetected(consequences, randomItemToRename, expectedOldName, expectedNewName);
        }

        [Fact, Description("Verifies if we detect if an existing unit has been moved to another existing unit")]
        public void ComputeUpdate_Detects_Units_Moved_To_Existing_Parent()
        {
            //Arrange
            var (newRoot, externalTree, randomLeafWhichMustBeMovedToRoot) = CreateTreeWithUnitsMovedToExistingParent();

            //Act
            var consequences = _sut.ComputeUpdate(externalTree);

            //Assert
            AssertUnitsToMoveToExistingParentsWereDetected(consequences, randomLeafWhichMustBeMovedToRoot, newRoot);
        }

        [Fact, Description("Verifies if we detect if an existing unit has been moved one of the new units")]
        public void ComputeUpdate_Detects_Units_Moved_To_Newly_Added_Parent()
        {
            //Arrange
            var (root, externalTree, randomLeafMovedToNewlyImportedItem, newItem) = CreateTreeWithUnitsMovedToNewlyAddedUnit();

            //Act
            var consequences = _sut.ComputeUpdate(externalTree);

            //Assert
            AssertUnitsToMoveToNewlyAddedParentWereDetected(consequences, root, newItem, randomLeafMovedToNewlyImportedItem);
        }

        [Fact]
        public void ComputeUpdate_Detects_Removed_Units_Which_Are_Converted_Since_They_Contain_Retained_SubTree_Content()
        {
            //Arrange
            var (_, externalTree, nodeExpectedToBeConverted, expectedRemovedUnits) = CreateTreeWithUnitsWhichAreConvertedSinceTheyContainRetainedSubTreeContent();

            //Act
            var consequences = _sut.ComputeUpdate(externalTree);

            //Assert
            AssertUnitsWhichAreConvertedSinceTheyContainRetainedSubTreeContentWereDetected(consequences, nodeExpectedToBeConverted, expectedRemovedUnits);
        }

        [Fact]
        public void ComputeUpdate_Detects_Removed_Units_Which_Are_Converted_Since_They_Are_Still_In_Use()
        {
            //Arrange
            var (_, externalTree, removedNodeInUse) = CreateTreeWithUnitsWhichAreConvertedSinceTheyAreStillInUse();

            //Act
            var consequences = _sut.ComputeUpdate(externalTree);

            //Assert
            AssertUnitsWhichAreConvertedSinceTheyAreStillInUseWereDetected(consequences, removedNodeInUse);
        }

        [Fact]
        public void ComputeUpdate_Detects_Removed_Units_Which_Are_Deleted()
        {
            //Arrange
            var (_, externalTree, expectedRemovedUnit) = CreateTreeWithUnitsWhichAreDeleted();


            //Act
            var consequences = _sut.ComputeUpdate(externalTree);

            //Assert
            AssertUnitsWhichAreDeletedWereDetected(consequences, expectedRemovedUnit);
        }

        [Fact]
        public void ComputeUpdate_Detects_Removed_Nodes_Where_Leafs_Are_Moved_To_Removed_UnitsParent()
        {
            //Arrange
            var (_, externalTree, expectedParentChangesIEnumerable, expectedRemovedUnit) = CreateTreeWithUnitsWhichLeafsAreMovedToRemovedUnitsParent();
            var expectedParentChanges = expectedParentChangesIEnumerable.ToList();

            //Act
            var consequences = _sut.ComputeUpdate(externalTree);

            //Assert
            var removedUnit = Assert.Single(consequences.DeletedExternalUnitsBeingDeleted).organizationUnit;
            Assert.Same(expectedRemovedUnit, removedUnit);
            var movedUnits = consequences.OrganizationUnitsBeingMoved.ToList();
            Assert.Equal(expectedParentChanges.Count, movedUnits.Count);
            foreach (var (movedUnit, oldParent, newParent) in movedUnits)
            {
                Assert.Equal(removedUnit, oldParent);
                Assert.Equal(removedUnit.Parent.ExternalOriginUuid.GetValueOrDefault(), newParent.Uuid);
                Assert.Contains(movedUnit, expectedParentChanges);
            }

            Assert.Empty(consequences.DeletedExternalUnitsBeingConvertedToNativeUnits);
            Assert.Empty(consequences.OrganizationUnitsBeingRenamed);
            Assert.Empty(consequences.AddedExternalOrganizationUnits);
        }

        [Fact]
        public void PerformUpdate_Updates_New_OrganizationUnits()
        {
            //Arrange
            var (root, externalTree, _, _, _, expectedNewUnits) = CreateTreeWithNewOrganizationUnits();

            //Act
            var consequences = _sut.PerformUpdate(externalTree);

            //Assert
            Assert.True(consequences.Ok);
            Assert.ProperSubset(root.FlattenHierarchy().Where(x => x.Origin == OrganizationUnitOrigin.STS_Organisation).Select(x => x.ExternalOriginUuid.GetValueOrDefault()).ToHashSet(), expectedNewUnits.Select(x => x.ExternalOriginUuid.GetValueOrDefault()).ToHashSet());
        }

        [Fact]
        public void PerformUpdate_Updates_Renamed_OrganizationUnits()
        {
            //Arrange
            var (root, externalTree, randomItemToRename, expectedOldName, expectedNewName) = CreateTreeWithRenamedOrganizationUnits();

            //Act
            var consequences = _sut.PerformUpdate(externalTree);

            //Assert
            Assert.True(consequences.Ok);
            Assert.Equal(expectedNewName, randomItemToRename.Name);
        }

        [Fact]
        public void PerformUpdate_Updates_Units_Moved_To_Existing_Parent()
        {
            //Arrange
            var (root, externalTree, randomLeafWhichMustBeMovedToRoot) = CreateTreeWithUnitsMovedToExistingParent();

            //Act
            var consequences = _sut.PerformUpdate(externalTree);

            //Assert
            Assert.True(consequences.Ok);
            Assert.Contains(randomLeafWhichMustBeMovedToRoot, root.Children);
        }

        [Fact]
        public void PerformUpdate_Updates_Units_Moved_To_Existing_Parent_And_Includes_Native_Children_Created_Moved_Item()
        {
            //Arrange
            var (root, externalTree, randomLeafWhichMustBeMovedToRoot) = CreateTreeWithUnitsMovedToExistingParent();
            var childExpectedToBeMoved = CreateOrganizationUnit(OrganizationUnitOrigin.Kitos);
            _organization.AddOrganizationUnit(childExpectedToBeMoved, randomLeafWhichMustBeMovedToRoot);

            //Act
            var consequences = _sut.PerformUpdate(externalTree);

            //Assert
            Assert.True(consequences.Ok);
            Assert.Contains(randomLeafWhichMustBeMovedToRoot, root.Children);
            Assert.Contains(childExpectedToBeMoved, randomLeafWhichMustBeMovedToRoot.Children);
        }

        [Fact]
        public void PerformUpdate_Updates_Units_Moved_To_Newly_Added_Parent()
        {
            //Arrange
            var (root, externalTree, randomLeafMovedToNewlyImportedItem, newItem) = CreateTreeWithUnitsMovedToNewlyAddedUnit();

            //Act
            var consequences = _sut.PerformUpdate(externalTree);

            //Assert
            Assert.True(consequences.Ok);
            Assert.Equal(newItem.ExternalOriginUuid.GetValueOrDefault(), randomLeafMovedToNewlyImportedItem.Parent.ExternalOriginUuid.GetValueOrDefault());
        }

        [Fact]
        public void PerformUpdate_Updates_Units_Moved_To_Newly_Added_Parent_And_Sub_Tree_Is_Moved_Along()
        {
            //Arrange
            var root = CreateOrganizationUnit(OrganizationUnitOrigin.STS_Organisation,
                new[]
                {
                    CreateOrganizationUnit(OrganizationUnitOrigin.STS_Organisation,
                        new []
                        {
                            CreateOrganizationUnit(OrganizationUnitOrigin.STS_Organisation,
                                new []
                                {
                                    CreateOrganizationUnit(OrganizationUnitOrigin.STS_Organisation,new[]
                                    {
                                        CreateOrganizationUnit(OrganizationUnitOrigin.STS_Organisation)
                                    })
                                }),
                            CreateOrganizationUnit(OrganizationUnitOrigin.STS_Organisation),
                            CreateOrganizationUnit(OrganizationUnitOrigin.STS_Organisation)
                        })
                });
            var externalTree = ConvertToExternalTree(root); //the complete tree
            var childToRemove = root.FlattenHierarchy().Skip(1).First();
            foreach (var grandChild in childToRemove.Children.ToList())
            {
                childToRemove.RemoveChild(grandChild);
                root.AddChild(grandChild);
            }
            root.RemoveChild(childToRemove); // the current tree is missing a link -> we expect the final tree to be 100% like the external tree including sub trees


            PrepareConnectedOrganization(root);

            //Act
            var consequences = _sut.PerformUpdate(externalTree);

            //Assert
            Assert.True(consequences.Ok);
            AssertHierarchies(externalTree, ConvertToExternalTree(_organization.GetRoot()));
        }

        private void AssertHierarchies(ExternalOrganizationUnit expected, ExternalOrganizationUnit actual, int level = 1)
        {
            _testOutputHelper.WriteLine("Testing hierarchy consistency at level {0} currently evaluating expected node:{1} ({2})", level, expected.Name, expected.Uuid);
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.Uuid, actual.Uuid);
            var expectedChildren = expected.Children.ToDictionary(x => x.Uuid);
            var actualChildren = actual.Children.ToDictionary(x => x.Uuid);
            Assert.Equivalent(expectedChildren.Keys, actualChildren.Keys, true);

            foreach (var expectedChild in expectedChildren)
            {
                AssertHierarchies(expectedChild.Value, actualChildren[expectedChild.Key], level + 1);
            }
        }

        [Fact]
        public void PerformUpdate_Updates_Removed_Units_Which_Are_Converted_Since_They_Contain_Retained_SubTree_Content()
        {
            //Arrange
            var (root, externalTree, nodeExpectedToBeConverted, expectedRemovedUnits) = CreateTreeWithUnitsWhichAreConvertedSinceTheyContainRetainedSubTreeContent();

            //Act
            var consequences = _sut.PerformUpdate(externalTree);

            //Assert
            Assert.True(consequences.Ok);
            Assert.DoesNotContain(root.FlattenHierarchy(), child => expectedRemovedUnits.Contains(child));
            var actualConverted = Assert.Single(root.FlattenHierarchy().Where(x => x == nodeExpectedToBeConverted));
            Assert.Equal(OrganizationUnitOrigin.Kitos, actualConverted.Origin);
            Assert.Null(actualConverted.ExternalOriginUuid);
        }

        [Fact]
        public void PerformUpdate_Updates_Removed_Units_Which_Are_Converted_Since_They_Are_Still_In_Use()
        {
            //Arrange
            var (root, externalTree, removedNodeInUse) = CreateTreeWithUnitsWhichAreConvertedSinceTheyAreStillInUse();

            //Act
            var consequences = _sut.PerformUpdate(externalTree);

            //Assert
            Assert.True(consequences.Ok);

            var expectedConversion = Assert.Single(root.FlattenHierarchy().Where(x => x == removedNodeInUse));
            Assert.Equal(OrganizationUnitOrigin.Kitos, expectedConversion.Origin);
            Assert.Null(expectedConversion.ExternalOriginUuid);
        }

        [Fact]
        public void PerformUpdate_Removes_Removed_Units_Which_Are_Deleted()
        {
            //Arrange
            var (root, externalTree, expectedRemovedUnit) = CreateTreeWithUnitsWhichAreDeleted();

            //Act
            var consequences = _sut.PerformUpdate(externalTree);

            //Assert
            Assert.True(consequences.Ok);
            Assert.DoesNotContain(expectedRemovedUnit, root.FlattenHierarchy());
        }

        [Fact]
        public void PerformUpdate_Removes_Removed_Nodes_Where_Leafs_Are_Moved_To_Removed_UnitsParent()
        {
            //Arrange
            var (root, externalTree, expectedParentChanges, expectedRemovedUnit) = CreateTreeWithUnitsWhichLeafsAreMovedToRemovedUnitsParent();
            var expectedParentChangesCount = expectedParentChanges.Count();

            //Act
            var consequences = _sut.PerformUpdate(externalTree);

            //Assert
            Assert.True(consequences.Ok);
            var removedUnit = Assert.Single(consequences.Value.DeletedExternalUnitsBeingDeleted).organizationUnit;
            Assert.Same(expectedRemovedUnit, removedUnit);
            var movedUnits = consequences.Value.OrganizationUnitsBeingMoved.ToList();
            Assert.Equal(expectedParentChangesCount, movedUnits.Count);
            foreach (var (affectedUnit, oldParent, newParent) in movedUnits)
            {
                Assert.Equal(removedUnit, oldParent);
                Assert.Equal(affectedUnit.Parent?.ExternalOriginUuid.GetValueOrDefault(), newParent.Uuid);
            }

            Assert.Empty(consequences.Value.DeletedExternalUnitsBeingConvertedToNativeUnits);
            Assert.Empty(consequences.Value.OrganizationUnitsBeingRenamed);
            Assert.Empty(consequences.Value.AddedExternalOrganizationUnits);

            var hierarchy = root.FlattenHierarchy();
            Assert.Null(hierarchy.FirstOrDefault(x => x.Uuid == removedUnit.Uuid));
        }

        private static void AssertNewUnitsWereDetected(OrganizationTreeUpdateConsequences consequences, IEnumerable<OrganizationUnit> expectedNewUnits, OrganizationUnit expectedSubTree, OrganizationUnit randomParentOfNewSubTree, OrganizationUnit expectedChild)
        {
            Assert.Empty(consequences.DeletedExternalUnitsBeingConvertedToNativeUnits);
            Assert.Empty(consequences.DeletedExternalUnitsBeingDeleted);
            Assert.Empty(consequences.OrganizationUnitsBeingMoved);
            Assert.Empty(consequences.OrganizationUnitsBeingRenamed);

            var addedUnits = consequences.AddedExternalOrganizationUnits.ToList();
            Assert.Equal(2, addedUnits.Count);
            Assert.Contains(addedUnits, unit => expectedNewUnits.Any(x => x.ExternalOriginUuid.GetValueOrDefault() == unit.unitToAdd.Uuid));

            var addedRoot = Assert.Single(addedUnits.Where(x => x.unitToAdd.Uuid == expectedSubTree.ExternalOriginUuid.GetValueOrDefault()));
            Assert.Equal(randomParentOfNewSubTree.ExternalOriginUuid.GetValueOrDefault(), addedRoot.parent.Uuid);
            var addedChild = Assert.Single(addedUnits.Where(x => x.unitToAdd.Uuid == expectedChild.ExternalOriginUuid.GetValueOrDefault()));
            Assert.Equal(addedRoot.unitToAdd, addedChild.parent);
        }

        private static void AssertUnitsToRenameWereDetected(OrganizationTreeUpdateConsequences consequences, OrganizationUnit randomItemToRename, string expectedOldName, string expectedNewName)
        {
            Assert.Empty(consequences.DeletedExternalUnitsBeingConvertedToNativeUnits);
            Assert.Empty(consequences.DeletedExternalUnitsBeingDeleted);
            Assert.Empty(consequences.OrganizationUnitsBeingMoved);
            Assert.Empty(consequences.AddedExternalOrganizationUnits);
            var (affectedUnit, oldName, newName) = Assert.Single(consequences.OrganizationUnitsBeingRenamed);
            Assert.Same(randomItemToRename, affectedUnit);
            Assert.Equal(expectedOldName, oldName);
            Assert.Equal(expectedNewName, newName);
        }

        private static void AssertUnitsToMoveToExistingParentsWereDetected(
            OrganizationTreeUpdateConsequences consequences, OrganizationUnit randomLeafWhichMustBeMovedToRoot,
            OrganizationUnit expectedNewParent)
        {
            Assert.Empty(consequences.DeletedExternalUnitsBeingConvertedToNativeUnits);
            Assert.Empty(consequences.DeletedExternalUnitsBeingDeleted);
            Assert.Empty(consequences.OrganizationUnitsBeingRenamed);
            Assert.Empty(consequences.AddedExternalOrganizationUnits);

            var (movedUnit, _, newParent) = Assert.Single(consequences.OrganizationUnitsBeingMoved);
            Assert.Equal(randomLeafWhichMustBeMovedToRoot, movedUnit);
            Assert.Equal(expectedNewParent.ExternalOriginUuid.GetValueOrDefault(), newParent.Uuid);
        }

        private static void AssertUnitsToMoveToNewlyAddedParentWereDetected(OrganizationTreeUpdateConsequences consequences, OrganizationUnit root, OrganizationUnit exptectedNewItem, OrganizationUnit expectedMovedUnit)
        {
            Assert.Empty(consequences.DeletedExternalUnitsBeingConvertedToNativeUnits);
            Assert.Empty(consequences.DeletedExternalUnitsBeingDeleted);
            Assert.Empty(consequences.OrganizationUnitsBeingRenamed);
            var (unitToAdd, parent) = Assert.Single(consequences.AddedExternalOrganizationUnits);
            Assert.Equal(exptectedNewItem.ExternalOriginUuid.GetValueOrDefault(), unitToAdd.Uuid);
            Assert.Equal(root.ExternalOriginUuid.GetValueOrDefault(), parent.Uuid);

            var (movedUnit, _, newParent) = Assert.Single(consequences.OrganizationUnitsBeingMoved);
            Assert.Equal(expectedMovedUnit, movedUnit);
            Assert.Equal(unitToAdd.Uuid, newParent.Uuid);
        }

        private static void AssertUnitsWhichAreConvertedSinceTheyContainRetainedSubTreeContentWereDetected(
            OrganizationTreeUpdateConsequences consequences, OrganizationUnit nodeExpectedToBeConverted,
            IEnumerable<OrganizationUnit> expectedRemovedUnits)
        {
            var organizationUnit = Assert.Single(consequences.DeletedExternalUnitsBeingConvertedToNativeUnits).organizationUnit;
            Assert.Same(nodeExpectedToBeConverted, organizationUnit);

            var expectedRemovedItems = expectedRemovedUnits.OrderBy(unit => unit.Id);
            var actualRemovedItems = consequences.DeletedExternalUnitsBeingDeleted.Select(x => x.organizationUnit).OrderBy(unit => unit.Id);
            Assert.Equal(expectedRemovedItems, actualRemovedItems);

            Assert.Empty(consequences.OrganizationUnitsBeingRenamed);
            Assert.Empty(consequences.AddedExternalOrganizationUnits);
            Assert.Empty(consequences.OrganizationUnitsBeingMoved);
        }

        private static void AssertUnitsWhichAreConvertedSinceTheyAreStillInUseWereDetected(
            OrganizationTreeUpdateConsequences consequences, OrganizationUnit removedNodeInUse)
        {
            var organizationUnit = Assert.Single(consequences.DeletedExternalUnitsBeingConvertedToNativeUnits).organizationUnit;
            Assert.Same(removedNodeInUse, organizationUnit);

            Assert.Empty(consequences.DeletedExternalUnitsBeingDeleted);
            Assert.Empty(consequences.OrganizationUnitsBeingRenamed);
            Assert.Empty(consequences.AddedExternalOrganizationUnits);
            Assert.Empty(consequences.OrganizationUnitsBeingMoved);
        }

        private static void AssertUnitsWhichAreDeletedWereDetected(OrganizationTreeUpdateConsequences consequences,
            OrganizationUnit expectedRemovedUnit)
        {
            var removedUnit = Assert.Single(consequences.DeletedExternalUnitsBeingDeleted).organizationUnit;
            Assert.Same(expectedRemovedUnit, removedUnit);

            Assert.Empty(consequences.DeletedExternalUnitsBeingConvertedToNativeUnits);
            Assert.Empty(consequences.OrganizationUnitsBeingRenamed);
            Assert.Empty(consequences.AddedExternalOrganizationUnits);
            Assert.Empty(consequences.OrganizationUnitsBeingMoved);
        }

        private static ExternalOrganizationUnit ConvertToExternalTree(OrganizationUnit root, Func<OrganizationUnit, IEnumerable<OrganizationUnit>, IEnumerable<OrganizationUnit>> customChildren = null)
        {
            customChildren ??= ((unit, existingChildren) => existingChildren);

            return new ExternalOrganizationUnit(
                root.ExternalOriginUuid.GetValueOrDefault(),
                root.Name,
                new Dictionary<string, string>(),
                root
                    .Children
                    .Where(x => x.Origin == OrganizationUnitOrigin.STS_Organisation)
                    .Transform(filteredChildren => customChildren(root, filteredChildren))
                    .Select(child => ConvertToExternalTree(child, customChildren))
                    .ToList()
                );
        }

        private (OrganizationUnit root, ExternalOrganizationUnit externalTree, OrganizationUnit randomParentOfNewSubTree, OrganizationUnit
            expectedSubTree, OrganizationUnit expectedChild, IEnumerable<OrganizationUnit> expectedNewUnits)
            CreateTreeWithNewOrganizationUnits()
        {
            PrepareConnectedOrganization();
            var root = _organization.GetRoot();

            var randomParentOfNewSubTree = root
                .FlattenHierarchy()
                .Skip(1) // Skip the root
                .Where(x => x.Origin == OrganizationUnitOrigin.STS_Organisation)
                .RandomItem();

            var expectedSubTree = CreateOrganizationUnit(
                OrganizationUnitOrigin.STS_Organisation,
                new[]
                {
                    CreateOrganizationUnit(OrganizationUnitOrigin.STS_Organisation)
                }
            );
            var expectedNewUnits = expectedSubTree.FlattenHierarchy().ToList();
            var expectedChild = expectedNewUnits.Skip(1).Single();

            var externalTree = ConvertToExternalTree(root, (current, currentChildren) =>
            {
                //Add the new sub tree if this is the parent of the new sub tree we expect
                if (current == randomParentOfNewSubTree)
                {
                    return expectedSubTree
                        .WrapAsEnumerable()
                        .Concat(currentChildren);
                }

                return currentChildren;
            });

            return (root, externalTree, randomParentOfNewSubTree, expectedSubTree, expectedChild, expectedNewUnits);
        }

        private (OrganizationUnit root, ExternalOrganizationUnit externalTree, OrganizationUnit randomItemToRename, string expectedOldName, string expectedNewName) CreateTreeWithRenamedOrganizationUnits()
        {
            PrepareConnectedOrganization();
            var root = _organization.GetRoot();

            var randomItemToRename = root
                .FlattenHierarchy()
                .Where(x => x.Origin == OrganizationUnitOrigin.STS_Organisation)
                .RandomItem();

            var externalTree = ConvertToExternalTree(root);

            var expectedNewName = randomItemToRename.Name; //as converted
            var expectedOldName = A<string>();
            randomItemToRename.Name = expectedOldName; //Rename the local item to enforce name change detection

            return (root, externalTree, randomItemToRename, expectedOldName, expectedNewName);
        }

        private (OrganizationUnit root, ExternalOrganizationUnit externalTree, OrganizationUnit randomLeafWhichMustBeMovedToRoot) CreateTreeWithUnitsMovedToExistingParent()
        {
            PrepareConnectedOrganization();
            var root = _organization.GetRoot();
            var randomLeafWhichMustBeMovedToRoot = root
                .FlattenHierarchy()
                .Where(x => x.Origin == OrganizationUnitOrigin.STS_Organisation)
                .Where(x => x.IsLeaf())
                .RandomItem();

            var externalTree = ConvertToExternalTree(root, (current, currentChildren) =>
            {
                if (current == randomLeafWhichMustBeMovedToRoot.Parent)
                {
                    //Remove from the current parent
                    return currentChildren.Where(child => child != randomLeafWhichMustBeMovedToRoot).ToList();
                }

                return current.IsRoot()
                    ? currentChildren.Append(randomLeafWhichMustBeMovedToRoot).ToList()
                    : currentChildren;
            });

            return (root, externalTree, randomLeafWhichMustBeMovedToRoot);
        }

        private (OrganizationUnit root, ExternalOrganizationUnit externalTree, OrganizationUnit randomLeafMovedToNewlyImportedItem, OrganizationUnit newItem) CreateTreeWithUnitsMovedToNewlyAddedUnit()
        {
            PrepareConnectedOrganization();
            var root = _organization.GetRoot();
            var randomLeafMovedToNewlyImportedItem = root
                .FlattenHierarchy()
                .Where(x => x.Origin == OrganizationUnitOrigin.STS_Organisation)
                .Where(x => x.IsLeaf())
                .RandomItem();

            var newItem = CreateOrganizationUnit(
                OrganizationUnitOrigin.STS_Organisation,
                new[]
                {
                    //NOTE: Make a copy to not modify the existing object (children in the list will get the parent in scope and this affects detection)
                    new OrganizationUnit
                    {
                        Id = randomLeafMovedToNewlyImportedItem.Id,
                        Organization = _organization,
                        Origin = randomLeafMovedToNewlyImportedItem.Origin,
                        ExternalOriginUuid = randomLeafMovedToNewlyImportedItem.ExternalOriginUuid,
                        Name = randomLeafMovedToNewlyImportedItem.Name
                    }
                }
            );
            newItem.Parent = root;

            var externalTree = ConvertToExternalTree(root, (current, currentChildren) =>
            {
                if (current == randomLeafMovedToNewlyImportedItem.Parent)
                {
                    //Remove from the current parent
                    return currentChildren.Where(child => child != randomLeafMovedToNewlyImportedItem).ToList();
                }

                if (current.IsRoot())
                {
                    //Add the new item to the root and the new item contains the moved item
                    return currentChildren.Append(newItem).ToList();
                }

                return currentChildren;
            });

            return (root, externalTree, randomLeafMovedToNewlyImportedItem, newItem);
        }

        private (OrganizationUnit root, ExternalOrganizationUnit externalTree, OrganizationUnit nodeExpectedToBeConverted, IEnumerable<OrganizationUnit> expectedRemovedUnits) CreateTreeWithUnitsWhichAreConvertedSinceTheyContainRetainedSubTreeContent()
        {
            PrepareConnectedOrganization();
            var root = _organization.GetRoot();
            var nodeExpectedToBeConverted = root
                .FlattenHierarchy()
                .Where(x => //Find a synced node that contains native children which are not deleted by external deletions. We expect the native child to be deleted (if any)
                    x != root &&
                    x.Origin == OrganizationUnitOrigin.STS_Organisation &&
                    x.Children.Any(c => c.Origin == OrganizationUnitOrigin.Kitos))
                .RandomItem();

            var subtreeOfRemovedExternalItem = nodeExpectedToBeConverted
                .FlattenHierarchy()
                .Where(node => node != nodeExpectedToBeConverted);
            var expectedRemovedUnits = subtreeOfRemovedExternalItem.Where(x => x.Origin == OrganizationUnitOrigin.STS_Organisation).ToList();

            var externalTree = ConvertToExternalTree(root, (_, currentChildren) =>
            {
                //Make sure the removed subtree is filtered out
                return currentChildren.Where(child => child != nodeExpectedToBeConverted);
            });

            return new ValueTuple<OrganizationUnit, ExternalOrganizationUnit, OrganizationUnit, IEnumerable<OrganizationUnit>>(root, externalTree, nodeExpectedToBeConverted, expectedRemovedUnits);
        }

        private (OrganizationUnit root, ExternalOrganizationUnit externalTree, OrganizationUnit nodeExpectedToBeConverted) CreateTreeWithUnitsWhichAreConvertedSinceTheyAreStillInUse()
        {
            PrepareConnectedOrganization();
            var root = _organization.GetRoot();
            var removedNodeInUse = root
                .FlattenHierarchy()
                .Where(x => //Find a synced node that contains native children which are not deleted by external deletions. We expect the native child to be deleted (if any)
                    x.Origin == OrganizationUnitOrigin.STS_Organisation &&
                    x.IsLeaf())
                .RandomItem();
            removedNodeInUse.Using.Add(new ItSystemUsageOrgUnitUsage());

            var externalTree = ConvertToExternalTree(root, (_, currentChildren) =>
            {
                //Make sure the removed subtree is filtered out
                return currentChildren.Where(child => child != removedNodeInUse);
            });

            return new ValueTuple<OrganizationUnit, ExternalOrganizationUnit, OrganizationUnit>(root, externalTree, removedNodeInUse);
        }

        private (OrganizationUnit root, ExternalOrganizationUnit externalTree, OrganizationUnit expectedRemovedUnit) CreateTreeWithUnitsWhichAreDeleted()
        {
            PrepareConnectedOrganization();
            var root = _organization.GetRoot();
            var expectedRemovedUnit = root
                .FlattenHierarchy()
                .Where(x => //Find a synced node that contains native children which are not deleted by external deletions. We expect the native child to be deleted (if any)
                    x.Origin == OrganizationUnitOrigin.STS_Organisation &&
                    x.IsLeaf())
                .RandomItem();

            var externalTree = ConvertToExternalTree(root, (_, currentChildren) =>
            {
                //Make sure the removed subtree is filtered out
                return currentChildren.Where(child => child != expectedRemovedUnit);
            });

            return new ValueTuple<OrganizationUnit, ExternalOrganizationUnit, OrganizationUnit>(root, externalTree, expectedRemovedUnit);
        }

        private (OrganizationUnit root, ExternalOrganizationUnit externalTree, IEnumerable<OrganizationUnit> expectedParentChanges, OrganizationUnit expectedRemovedUnit) CreateTreeWithUnitsWhichLeafsAreMovedToRemovedUnitsParent()
        {
            PrepareConnectedOrganization();
            var root = _organization.GetRoot();
            var expectedRemovedUnit = root
                .FlattenHierarchy()
                .Where(x => //Find a synced node that contains native children which are not deleted by external deletions. We expect the native child to be deleted (if any)
                    x != root &&
                    x.Origin == OrganizationUnitOrigin.STS_Organisation &&
                    !x.IsLeaf() &&
                    x.FlattenHierarchy().All(c => c.Origin == OrganizationUnitOrigin.STS_Organisation))
                .RandomItem();

            var expectedParentChanges = expectedRemovedUnit.Children;

            var externalTree = ConvertToExternalTree(root, (current, currentChildren) =>
            {
                if (current == expectedRemovedUnit.Parent)
                {
                    return currentChildren
                        .Where(child => child != expectedRemovedUnit)
                        //Move the children of the removed item to the removed item's parent
                        .Concat(expectedParentChanges.Select(x => new OrganizationUnit
                        {
                            Id = x.Id,
                            Organization = _organization,
                            ExternalOriginUuid = x.ExternalOriginUuid,
                            Origin = x.Origin,
                            Children = x.Children,
                            Name = x.Name
                        })).ToList();
                }
                return currentChildren;
            });

            return new ValueTuple<OrganizationUnit, ExternalOrganizationUnit, IEnumerable<OrganizationUnit>, OrganizationUnit>(root, externalTree, expectedParentChanges, expectedRemovedUnit);
        }
    }
}
